-- Sample data for VoxMed application (Development only)
-- This script populates the database with sample data for testing and development

-- Sample doctors
INSERT INTO users.users (email, password_hash, first_name, last_name, phone, role_id, is_active, email_verified)
SELECT 
    'dr.smith@voxmed.com',
    crypt('doctor123', gen_salt('bf')),
    'John',
    'Smith',
    '+1-555-0101',
    r.id,
    true,
    true
FROM users.roles r
WHERE r.name = 'doctor'
ON CONFLICT (email) DO NOTHING;

INSERT INTO users.users (email, password_hash, first_name, last_name, phone, role_id, is_active, email_verified)
SELECT 
    'dr.johnson@voxmed.com',
    crypt('doctor123', gen_salt('bf')),
    'Sarah',
    'Johnson',
    '+1-555-0102',
    r.id,
    true,
    true
FROM users.roles r
WHERE r.name = 'doctor'
ON CONFLICT (email) DO NOTHING;

-- Sample nurse
INSERT INTO users.users (email, password_hash, first_name, last_name, phone, role_id, is_active, email_verified)
SELECT 
    'nurse.williams@voxmed.com',
    crypt('nurse123', gen_salt('bf')),
    'Mary',
    'Williams',
    '+1-555-0201',
    r.id,
    true,
    true
FROM users.roles r
WHERE r.name = 'nurse'
ON CONFLICT (email) DO NOTHING;

-- Sample receptionist
INSERT INTO users.users (email, password_hash, first_name, last_name, phone, role_id, is_active, email_verified)
SELECT 
    'reception@voxmed.com',
    crypt('reception123', gen_salt('bf')),
    'Lisa',
    'Brown',
    '+1-555-0301',
    r.id,
    true,
    true
FROM users.roles r
WHERE r.name = 'receptionist'
ON CONFLICT (email) DO NOTHING;

-- Sample patients
INSERT INTO medical.patients (patient_number, first_name, last_name, date_of_birth, gender, phone, email, address, emergency_contact, allergies, created_by)
SELECT 
    'P001',
    'Alice',
    'Cooper',
    '1985-03-15',
    'Female',
    '+1-555-1001',
    'alice.cooper@email.com',
    '{"street": "123 Main St", "city": "Anytown", "state": "CA", "zip": "12345"}'::jsonb,
    '{"name": "Bob Cooper", "relationship": "Spouse", "phone": "+1-555-1002"}'::jsonb,
    ARRAY['Penicillin', 'Shellfish'],
    u.id
FROM users.users u
WHERE u.email = 'admin@voxmed.com'
ON CONFLICT (patient_number) DO NOTHING;

INSERT INTO medical.patients (patient_number, first_name, last_name, date_of_birth, gender, phone, email, address, emergency_contact, allergies, created_by)
SELECT 
    'P002',
    'Robert',
    'Davis',
    '1978-07-22',
    'Male',
    '+1-555-1003',
    'robert.davis@email.com',
    '{"street": "456 Oak Ave", "city": "Somewhere", "state": "NY", "zip": "67890"}'::jsonb,
    '{"name": "Jennifer Davis", "relationship": "Wife", "phone": "+1-555-1004"}'::jsonb,
    ARRAY['None known'],
    u.id
FROM users.users u
WHERE u.email = 'admin@voxmed.com'
ON CONFLICT (patient_number) DO NOTHING;

INSERT INTO medical.patients (patient_number, first_name, last_name, date_of_birth, gender, phone, email, address, emergency_contact, allergies, created_by)
SELECT 
    'P003',
    'Emma',
    'Wilson',
    '1992-11-08',
    'Female',
    '+1-555-1005',
    'emma.wilson@email.com',
    '{"street": "789 Pine Rd", "city": "Elsewhere", "state": "FL", "zip": "54321"}'::jsonb,
    '{"name": "David Wilson", "relationship": "Father", "phone": "+1-555-1006"}'::jsonb,
    ARRAY['Latex'],
    u.id
FROM users.users u
WHERE u.email = 'admin@voxmed.com'
ON CONFLICT (patient_number) DO NOTHING;

-- Sample appointments (future dates)
INSERT INTO medical.appointments (patient_id, doctor_id, appointment_date, duration_minutes, status, type, notes)
SELECT 
    p.id,
    d.id,
    NOW() + INTERVAL '2 days',
    60,
    'scheduled',
    'General Checkup',
    'Annual physical examination'
FROM medical.patients p, users.users d
WHERE p.patient_number = 'P001' AND d.email = 'dr.smith@voxmed.com';

INSERT INTO medical.appointments (patient_id, doctor_id, appointment_date, duration_minutes, status, type, notes)
SELECT 
    p.id,
    d.id,
    NOW() + INTERVAL '5 days',
    30,
    'scheduled',
    'Follow-up',
    'Follow-up on previous blood work'
FROM medical.patients p, users.users d
WHERE p.patient_number = 'P002' AND d.email = 'dr.johnson@voxmed.com';

INSERT INTO medical.appointments (patient_id, doctor_id, appointment_date, duration_minutes, status, type, notes)
SELECT 
    p.id,
    d.id,
    NOW() + INTERVAL '1 week',
    45,
    'scheduled',
    'Consultation',
    'New patient consultation'
FROM medical.patients p, users.users d
WHERE p.patient_number = 'P003' AND d.email = 'dr.smith@voxmed.com';

-- Sample completed appointments (past dates)
INSERT INTO medical.appointments (patient_id, doctor_id, appointment_date, duration_minutes, status, type, notes)
SELECT 
    p.id,
    d.id,
    NOW() - INTERVAL '1 month',
    30,
    'completed',
    'Routine Checkup',
    'Routine checkup - patient doing well'
FROM medical.patients p, users.users d
WHERE p.patient_number = 'P001' AND d.email = 'dr.smith@voxmed.com';

-- Sample medical records
INSERT INTO medical.medical_records (patient_id, appointment_id, doctor_id, record_type, diagnosis, symptoms, treatment_plan, medications, notes)
SELECT 
    p.id,
    a.id,
    d.id,
    'General Examination',
    '{"primary": "Hypertension (Stage 1)", "secondary": ["Mild anxiety"], "icd10": ["I10", "F41.1"]}'::jsonb,
    ARRAY['Elevated blood pressure', 'Occasional headaches', 'Mild fatigue'],
    '{"lifestyle": ["Reduce sodium intake", "Increase physical activity", "Stress management"], "followup": "3 months", "monitoring": ["Blood pressure", "Weight"]}'::jsonb,
    '{"prescribed": [{"name": "Lisinopril", "dosage": "10mg", "frequency": "Once daily", "duration": "3 months"}], "discontinued": []}'::jsonb,
    'Patient responded well to treatment recommendations. Blood pressure improved with lifestyle modifications.'
FROM medical.patients p, medical.appointments a, users.users d
WHERE p.patient_number = 'P001' 
  AND a.patient_id = p.id 
  AND a.status = 'completed'
  AND d.email = 'dr.smith@voxmed.com'
LIMIT 1;

-- Update patient medical history
UPDATE medical.patients 
SET medical_history = '{
    "chronic_conditions": ["Hypertension"],
    "surgeries": [],
    "family_history": {
        "heart_disease": "Father - age 65",
        "diabetes": "Mother - age 70"
    },
    "social_history": {
        "smoking": "Never",
        "alcohol": "Occasional",
        "exercise": "2-3 times per week"
    }
}'::jsonb,
current_medications = '[
    {
        "name": "Lisinopril",
        "dosage": "10mg",
        "frequency": "Once daily",
        "prescriber": "Dr. Smith",
        "start_date": "2025-07-25"
    }
]'::jsonb
WHERE patient_number = 'P001';
