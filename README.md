# Student Exam System - Backend

This repository contains the backend API for the Student Exam System, a web application for managing students, subjects, exam registrations and grading processes in a university environment.

The system models a simplified version of real academic systems such as eStudent.

The backend is implemented as a REST API using ASP.NET Core and Entity Framework Core.

The frontend application is available here:
https://github.com/anndjella/student-exam-system-front

---
# Architecture

The backend follows a layered architecture inspired by Clean Architecture / Onion Architecture.

Project structure:
- Api
- Application
- Domain
- Infrastructure

## Domain

Contains the core business entities and domain logic.

Main entities include:
- User
- Person
- Student
- Teacher
- Subject
- Enrollment
- Registration
- Exam
- Term
- TeachingAssignment

Some calculated values such as GPA and ECTSCount are retrieved through database views, while BirthDate is derived from the JMBG identifier.

---

## Application

Contains application services, DTOs, validators and business logic.

Responsibilities include:
- managing exam registrations
- validating domain operations
- orchestrating workflows
- coordinating repositories and domain entities

---

## Infrastructure

Contains persistence logic:

- Entity Framework Core
- repositories
- database context
- migrations
- some seeders

---

## API

Contains:
- controllers
- authentication configuration
- dependency injection configuration
- middleware configuration

---

# Roles
The system supports three user roles:
- Student Service
- Student
- Teacher
## Student Service
Administrative role responsible for managing the academic system.
Capabilities include:

- creating and updating students and teachers
- viewing all students and teachers, including soft deleted records
- managing subjects
- creating and viewing all subjects, including inactive subjects
- deleting or deactivating subjects
- assigning teachers to subjects and managing teaching assignments
- managing exam terms (create and view all terms)
- creating enrollments (both individual and bulk)
- viewing all enrollments
- viewing all exam registrations
- viewing all exams
- searching students by index number
- searching teachers by employee number

Student service can also perform soft delete operations on students and teachers.

---

## Student

Students can:
- view subjects they are enrolled in
- view passed subjects
- view subjects they still need to pass
- register exams
- cancel exam registrations
- view active exam registrations
- view passed exams
- view failed exam attempts
- view current and upcoming exam terms

Students may register exams only for subjects they are currently enrolled in and have not passed.

---

## Teacher

Teachers can:
- view students
- view subjects they teach
- view exam registrations for those subjects
- enter exam results for subjects that they have a permission to enter
- update exam
- lock exams once grading is complete
- see exam history for subjects they teach
- view current and upcoming exam terms

Only teachers with CanGrade = true are allowed to enter grades.

## User Profile

All users can access their profile information, which includes their personal details and account information.

---

# Core Domain Concepts
## Enrollment

Represents that a student attends a subject.

An enrollment record indicates whether the student has already passed the subject or not.

When a teacher finalizes and signs exam results for a subject, the enrollment is updated if the student has successfully passed the exam.

---
## Registration

Represents an exam application.

Students can register exams only during the registration period of a term.

Registration can be active or not.

Students may cancel registrations while the registration period is still open.

---
## Exam

Represents the result of an exam attempt for a specific student, subject, and term.

Exam contains:

- grade
- exam date
- notes
- signing timestamp

Grades are nullable because a student may register an exam but not attend it.

Teachers can enter and update exam results before the exam is finalized.  
If a teacher modifies an already entered grade, a note must be provided explaining the change.

While exam results are not yet signed, students can see the entered grade within the registered exams section.

Once the teacher signs the exam results:

- if the grade is greater than 5, the exam is considered passed and appears in the student's passed exams section
- if the grade is 5, the exam appears in the student's failed attempts
- if the grade is null, it means the student did not attend the exam and it is displayed as N.I. (Not Attended). It also appears in the student's failed attempts

---
## Term

Represents an exam period.

Each term has two time windows:

- exam duration

- registration period

Students may only register exams during the registration window.

Teachers cannot enter grades while the registration period is still active, and they cannot enter grades for the next term while there are unsigned exams from the previous term.

---
# Authentication

Authentication uses JWT tokens.

User accounts are automatically generated when students or teachers are created.

Username generation examples:

Student:
ds20210567

Teacher:
ms20170123

Initial password format:
PasS!{JMBG}

Users must change their password during their first login.

---
# Technology Stack
- ASP.NET Core
- Entity Framework Core
- SQL Server
---
# Running the Project

Configure the database connection string in appsettings.json.

Run migrations:
dotnet ef database update

Start the API:
dotnet run

The API will run on:

http://localhost:5000

---
# Future Improvements

Potential improvements include:

- analytics and reporting features
- exam statistics visualizations