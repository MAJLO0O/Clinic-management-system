# 🏥 Clinic Management Database Project

This project focuses on designing and working with a relational database for a clinic management system.
It includes a structured database schema, data generator, and initial performance considerations.

---

## 🚀 Features

* Relational database design for clinic operations
* Custom data generator for populating the database
* Support for generating large datasets (100–5000+ records)
* Batch inserts for improved performance
* Basic indexing for query optimization

---

## 🧱 Database Design

The system includes a relational schema with over 10 tables:

* `patient`
* `doctor`
* `specialization`
* `doctor_specialization`
* `appointment`
* `appointment_status`
* `payment`
* `payment_status`
* `medical_record`
* `branch`

### Key aspects:

* One-to-many and many-to-many relationships
* Foreign keys and constraints for data integrity
* Normalized database structure

---

## ⚙️ Data Generator

The project includes a custom data generator that:

* Generates realistic data for key entities
* Uses batch inserts to improve performance
* Prevents duplicates using HashSet and database constraints
* Generates only missing data to avoid duplication

---

## 📊 Indexes

Basic indexes have been added to improve query performance:

```sql
CREATE INDEX idx_appointment_patient ON appointment(patient_id);
CREATE INDEX idx_appointment_doctor ON appointment(doctor_id);
CREATE INDEX idx_payment_status ON payment(status_id);
```

---

## 🛠 Tech Stack

* C#
* Dapper
* PostgreSQL

---

## ⚙️ Running the Project

### ⚠️ Configuration

* Update connection string in `appsettings.json`
* Ensure PostgreSQL database is running

### Steps

```bash
git clone https://github.com/your-username/clinic-management-system.git
dotnet run
```

---

## 📌 Future Improvements

* Backend application layer (ASP.NET Core)
* Business logic implementation (services, validation)
* REST API for managing clinic operations
* Transaction handling for critical operations
* Database performance analysis (query optimization)
* UI for data visualization and interaction

---

## 🎯 Purpose

This project was created to practice:

* relational database design
* working with SQL and Dapper
* generating and handling large datasets
* basic database optimization

---

## 👨‍💻 Author

Junior backend developer focused on building backend systems in .NET and working with relational databases.
