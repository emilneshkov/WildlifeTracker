# Wildlife Population Monitoring System

A web application for monitoring and analyzing the population dynamics of wild animals in the Veliko Tarnovo region.

## Overview

The system allows volunteers to record annual changes in wildlife populations for specific settlements, while employees can generate statistical reports and analyze population trends across municipalities and the entire region.

The goal of the project is to support wildlife monitoring and provide structured data for analysis and decision-making.

## Features

### Volunteer
- Record annual population changes for wildlife species
- View personal submission history
- Data validation to prevent invalid population changes

### Employee
- View population matrix by settlements and species
- Generate reports for:
  - Total animals by municipality
  - Total animals by species in the region
  - Population growth compared to the previous year
  - Endangered species detection

### System
- Role-based access control (Volunteer / Employee)
- Data validation and integrity checks
- Pre-seeded data for municipalities, settlements, species and users
- Responsive UI built with Bootstrap

## Technologies Used

- **ASP.NET Core MVC (.NET 8)**
- **Entity Framework Core**
- **ASP.NET Identity**
- **Bootstrap**
- **SQL Server**

## Data Model

The system tracks wildlife population data using the following main entities:

- **Municipality**
- **Settlement**
- **Species**
- **InitialPopulation**
- **PopulationChange**
- **ApplicationUser**

Population changes are recorded annually and combined with the initial population to calculate the current population.

---