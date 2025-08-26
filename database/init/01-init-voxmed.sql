-- VoxMed Database Initialization Script
-- This script sets up the initial database structure for the VoxMed application

-- Enable necessary extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create schemas
CREATE SCHEMA IF NOT EXISTS medical;
CREATE SCHEMA IF NOT EXISTS users;
CREATE SCHEMA IF NOT EXISTS audit;

-- Set search path
SET search_path TO public, medical, users, audit;

-- Create audit table for tracking changes
CREATE TABLE IF NOT EXISTS audit.audit_log (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    table_name VARCHAR(100) NOT NULL,
    operation VARCHAR(10) NOT NULL,
    old_data JSONB,
    new_data JSONB,
    user_id UUID,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create users schema tables
CREATE TABLE IF NOT EXISTS users.roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(50) UNIQUE NOT NULL,
    description TEXT,
    permissions JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS users.users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    is_active BOOLEAN DEFAULT true,
    email_verified BOOLEAN DEFAULT false,
    role_id UUID REFERENCES users.roles(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_login TIMESTAMP WITH TIME ZONE
);

-- Create medical schema tables
CREATE TABLE IF NOT EXISTS medical.patients (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    patient_number VARCHAR(50) UNIQUE NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    date_of_birth DATE NOT NULL,
    gender VARCHAR(10),
    phone VARCHAR(20),
    email VARCHAR(255),
    address JSONB,
    emergency_contact JSONB,
    medical_history JSONB,
    allergies TEXT[],
    current_medications JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID REFERENCES users.users(id)
);

CREATE TABLE IF NOT EXISTS medical.appointments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    patient_id UUID NOT NULL REFERENCES medical.patients(id),
    doctor_id UUID NOT NULL REFERENCES users.users(id),
    appointment_date TIMESTAMP WITH TIME ZONE NOT NULL,
    duration_minutes INTEGER DEFAULT 30,
    status VARCHAR(20) DEFAULT 'scheduled',
    type VARCHAR(50),
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS medical.medical_records (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    patient_id UUID NOT NULL REFERENCES medical.patients(id),
    appointment_id UUID REFERENCES medical.appointments(id),
    doctor_id UUID NOT NULL REFERENCES users.users(id),
    record_type VARCHAR(50) NOT NULL,
    diagnosis JSONB,
    symptoms TEXT[],
    treatment_plan JSONB,
    medications JSONB,
    notes TEXT,
    attachments JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Insert default roles
INSERT INTO users.roles (name, description, permissions) VALUES
('admin', 'System Administrator', '{"all": true}'::jsonb),
('doctor', 'Medical Doctor', '{"patients": ["read", "write"], "appointments": ["read", "write"], "medical_records": ["read", "write"]}'::jsonb),
('nurse', 'Nurse', '{"patients": ["read"], "appointments": ["read", "write"], "medical_records": ["read"]}'::jsonb),
('receptionist', 'Receptionist', '{"patients": ["read", "write"], "appointments": ["read", "write"]}'::jsonb)
ON CONFLICT (name) DO NOTHING;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_email ON users.users(email);
CREATE INDEX IF NOT EXISTS idx_users_role_id ON users.users(role_id);
CREATE INDEX IF NOT EXISTS idx_patients_patient_number ON medical.patients(patient_number);
CREATE INDEX IF NOT EXISTS idx_patients_email ON medical.patients(email);
CREATE INDEX IF NOT EXISTS idx_appointments_patient_id ON medical.appointments(patient_id);
CREATE INDEX IF NOT EXISTS idx_appointments_doctor_id ON medical.appointments(doctor_id);
CREATE INDEX IF NOT EXISTS idx_appointments_date ON medical.appointments(appointment_date);
CREATE INDEX IF NOT EXISTS idx_medical_records_patient_id ON medical.medical_records(patient_id);
CREATE INDEX IF NOT EXISTS idx_medical_records_doctor_id ON medical.medical_records(doctor_id);
CREATE INDEX IF NOT EXISTS idx_audit_log_table_name ON audit.audit_log(table_name);
CREATE INDEX IF NOT EXISTS idx_audit_log_created_at ON audit.audit_log(created_at);

-- Create updated_at trigger function
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers for updated_at columns
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users.users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_roles_updated_at BEFORE UPDATE ON users.roles
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_patients_updated_at BEFORE UPDATE ON medical.patients
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_appointments_updated_at BEFORE UPDATE ON medical.appointments
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_medical_records_updated_at BEFORE UPDATE ON medical.medical_records
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Create audit trigger function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'DELETE' THEN
        INSERT INTO audit.audit_log (table_name, operation, old_data)
        VALUES (TG_TABLE_NAME, TG_OP, row_to_json(OLD));
        RETURN OLD;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO audit.audit_log (table_name, operation, old_data, new_data)
        VALUES (TG_TABLE_NAME, TG_OP, row_to_json(OLD), row_to_json(NEW));
        RETURN NEW;
    ELSIF TG_OP = 'INSERT' THEN
        INSERT INTO audit.audit_log (table_name, operation, new_data)
        VALUES (TG_TABLE_NAME, TG_OP, row_to_json(NEW));
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create audit triggers for important tables
CREATE TRIGGER audit_users_trigger
    AFTER INSERT OR UPDATE OR DELETE ON users.users
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_patients_trigger
    AFTER INSERT OR UPDATE OR DELETE ON medical.patients
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_medical_records_trigger
    AFTER INSERT OR UPDATE OR DELETE ON medical.medical_records
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

-- Create a default admin user (password: admin123)
-- Note: In production, this should be changed immediately
INSERT INTO users.users (email, password_hash, first_name, last_name, role_id, is_active, email_verified)
SELECT 
    'admin@voxmed.com',
    crypt('admin123', gen_salt('bf')),
    'System',
    'Administrator',
    r.id,
    true,
    true
FROM users.roles r
WHERE r.name = 'admin'
ON CONFLICT (email) DO NOTHING;

-- Grant appropriate permissions
GRANT USAGE ON SCHEMA medical TO voxmed;
GRANT USAGE ON SCHEMA users TO voxmed;
GRANT USAGE ON SCHEMA audit TO voxmed;

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA medical TO voxmed;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA users TO voxmed;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA audit TO voxmed;

GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA medical TO voxmed;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA users TO voxmed;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA audit TO voxmed;
