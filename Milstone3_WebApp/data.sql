USE HR_Payroll_System;
GO

-- Insert Roles (if they don't exist)
-- Using MERGE to avoid duplicates if roles already exist.
-- These role_name values must match the roles the application checks
-- (SystemAdmin, HRAdmin, LineManager, Employee, etc.).
MERGE Role AS target
USING (VALUES
    ('HRAdmin'),
    ('LineManager'),
    ('Employee'),
    ('Executive'),
    ('PayrollFinance'),
    ('Recruiter'),
    ('SystemAdmin')
) AS source (role_name)
ON target.role_name = source.role_name
WHEN NOT MATCHED THEN
    INSERT (role_name) VALUES (source.role_name);
GO

-- Insert Positions (if they don't exist)
-- Using MERGE to avoid duplicates if positions already exist

MERGE Position AS target
USING (VALUES
    ('Software Engineer', 'Develop and maintain software applications', 'Active'),
    ('Senior Software Engineer', 'Lead development projects and mentor junior developers', 'Active'),
    ('Product Manager', 'Manage product development and strategy', 'Active'),
    ('HR Manager', 'Oversee human resources operations and policies', 'Active'),
    ('Financial Analyst', 'Analyze financial data and prepare reports', 'Active'),
    ('Marketing Specialist', 'Develop and execute marketing campaigns', 'Active'),
    ('Sales Representative', 'Build client relationships and drive sales', 'Active'),
    ('Operations Manager', 'Oversee daily operations and improve efficiency', 'Active'),
    ('Data Analyst', 'Analyze data to provide business insights', 'Active'),
    ('Quality Assurance Engineer', 'Test software and ensure quality standards', 'Active')
) AS source (position_title, responsibilities, status)
ON target.position_title = source.position_title
WHEN NOT MATCHED THEN
    INSERT (position_title, responsibilities, status, created_at)
    VALUES (source.position_title, source.responsibilities, source.status, GETDATE());
GO

-- Insert 10 Employees with different positions
-- Using MERGE to avoid duplicates if employees already exist
-- Note: Adjust department_id, contract_id, and other foreign keys based on your existing data
-- For this example, we'll use NULL for optional fields that may not exist yet

MERGE Employee AS target
USING (VALUES
    ('EMP001', 'Ahmed', 'Ali', 'Ahmed Ali', 'ahmed.ali@company.com', '+201234567890', '12345678901234', '1990-05-15', 'Egypt', '2023-01-15', 'Software Engineer', 15000.00, '123 Main Street, Cairo', 'Fatima Ali', '+201234567891', 'Spouse'),
    ('EMP002', 'Mohamed', 'Hassan', 'Mohamed Hassan', 'mohamed.hassan@company.com', '+201234567892', '12345678901235', '1988-03-20', 'Egypt', '2022-06-01', 'Senior Software Engineer', 25000.00, '456 Oak Avenue, Alexandria', 'Sara Hassan', '+201234567893', 'Spouse'),
    ('EMP003', 'Sara', 'Ibrahim', 'Sara Ibrahim', 'sara.ibrahim@company.com', '+201234567894', '12345678901236', '1992-07-10', 'Egypt', '2023-03-10', 'Product Manager', 22000.00, '789 Pine Road, Giza', 'Omar Ibrahim', '+201234567895', 'Brother'),
    ('EMP004', 'Fatima', 'Mahmoud', 'Fatima Mahmoud', 'fatima.mahmoud@company.com', '+201234567896', '12345678901237', '1985-11-25', 'Egypt', '2021-09-01', 'HR Manager', 28000.00, '321 Elm Street, Cairo', 'Ahmed Mahmoud', '+201234567897', 'Husband'),
    ('EMP005', 'Omar', 'Khalil', 'Omar Khalil', 'omar.khalil@company.com', '+201234567898', '12345678901238', '1991-02-14', 'Egypt', '2023-05-20', 'Financial Analyst', 18000.00, '654 Maple Drive, Alexandria', 'Layla Khalil', '+201234567899', 'Sister'),
    ('EMP006', 'Layla', 'Youssef', 'Layla Youssef', 'layla.youssef@company.com', '+201234567900', '12345678901239', '1993-09-08', 'Egypt', '2023-07-15', 'Marketing Specialist', 16000.00, '987 Cedar Lane, Giza', 'Youssef Ali', '+201234567901', 'Father'),
    ('EMP007', 'Youssef', 'Nasser', 'Youssef Nasser', 'youssef.nasser@company.com', '+201234567902', '12345678901240', '1989-12-30', 'Egypt', '2022-11-01', 'Sales Representative', 14000.00, '147 Birch Street, Cairo', 'Mona Nasser', '+201234567903', 'Wife'),
    ('EMP008', 'Mona', 'Farid', 'Mona Farid', 'mona.farid@company.com', '+201234567904', '12345678901241', '1987-04-18', 'Egypt', '2021-12-10', 'Operations Manager', 26000.00, '258 Spruce Avenue, Alexandria', 'Tarek Farid', '+201234567905', 'Husband'),
    ('EMP009', 'Tarek', 'Said', 'Tarek Said', 'tarek.said@company.com', '+201234567906', '12345678901242', '1994-06-22', 'Egypt', '2023-09-01', 'Data Analyst', 17000.00, '369 Willow Road, Giza', 'Nour Said', '+201234567907', 'Sister'),
    ('EMP010', 'Nour', 'Adel', 'Nour Adel', 'nour.adel@company.com', '+201234567908', '12345678901243', '1992-10-05', 'Egypt', '2023-02-14', 'Quality Assurance Engineer', 15500.00, '741 Ash Street, Cairo', 'Karim Adel', '+201234567909', 'Brother')
) AS source (employee_code, first_name, last_name, full_name, email, phone, national_id, date_of_birth, country_of_birth, hire_date, position_title, base_salary, address, emergency_contact_name, emergency_contact_phone, relationship)
ON target.employee_code = source.employee_code
WHEN NOT MATCHED THEN
    INSERT (
        employee_code,
        first_name,
        last_name,
        full_name,
        email,
        phone,
        national_id,
        date_of_birth,
        country_of_birth,
        hire_date,
        position_id,
        department_id,
        manager_id,
        contract_id,
        pay_grade_id,
        salary_type_id,
        currency_id,
        base_salary,
        is_active,
        profile_completion,
        employment_status,
        account_status,
        address,
        emergency_contact_name,
        emergency_contact_phone,
        relationship,
        created_at
    )
    VALUES (
        source.employee_code,
        source.first_name,
        source.last_name,
        source.full_name,
        source.email,
        source.phone,
        source.national_id,
        source.date_of_birth,
        source.country_of_birth,
        source.hire_date,
        (SELECT position_id FROM Position WHERE position_title = source.position_title),
        NULL, -- department_id
        NULL, -- manager_id
        NULL, -- contract_id
        NULL, -- pay_grade_id
        NULL, -- salary_type_id
        NULL, -- currency_id
        source.base_salary,
        1, -- is_active
        1, -- profile_completion
        'Active', -- employment_status
        'Active', -- account_status
        source.address,
        source.emergency_contact_name,
        source.emergency_contact_phone,
        source.relationship,
        GETDATE()
    );

GO

-- Display inserted employees
SELECT 
    e.employee_id,
    e.employee_code,
    e.full_name,
    e.email,
    e.phone,
    p.position_title,
    e.hire_date,
    e.base_salary,
    e.employment_status
FROM Employee e
LEFT JOIN Position p ON e.position_id = p.position_id
WHERE e.employee_code IN ('EMP001', 'EMP002', 'EMP003', 'EMP004', 'EMP005', 'EMP006', 'EMP007', 'EMP008', 'EMP009', 'EMP010')
ORDER BY e.employee_id;

GO

