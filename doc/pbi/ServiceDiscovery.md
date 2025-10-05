# Technical Specification for the Development of an Automatic Service Discovery System

Develop a mechanism for automatic service discovery in various execution environments: development environment, production environment, and Docker environment. The system should provide dynamic determination of available services without the need for manual configuration.

- Provide automatic service discovery in real-time
- Implement cross-platform operation in various execution environments:
    - Development environment (single computer),
    - Production environment, distributed system within a single subnet,
    - Docker compose, swarm
- Minimize manual configuration
- Ensure fault tolerance and self-recovery of the system
- Services may support various protocols http, grpc

## Technical Requirements

### Functional Requirements

- Services should discover each other automatically upon startup
- The system should support dynamic addition and removal of services
- Services should periodically confirm their availability
- The mechanism should work in isolated network environments (Docker network)
- Provide handling of temporary service unavailability
- Use UDP broadcast to announce service availability
- Determine the broadcast address automatically for each environment
- Support standard ports for broadcast messages
- The system should detect service crashes
- Correct deregistration upon service shutdown
- There should be protection against replay attacks
- There should be the ability to manually add a service to the Registry of Available Services in case of network problems during discovery. For such services, deregistration does not occur in case of inactivity.

### System Components

- Broadcast Module
- Message Reception and Processing Module
- Registry of Available Services
- Health and Availability Check Mechanism
- Interface for Integration with Business Logic

# Code Requirements

Development language csharp, aspnet core
Use filescope namespace