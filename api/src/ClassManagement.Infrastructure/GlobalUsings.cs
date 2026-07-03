global using ClassManagement.Application.Interfaces.Audit;
global using ClassManagement.Application.Interfaces.Cache;
global using ClassManagement.Application.Interfaces.Email;
global using ClassManagement.Application.Interfaces.Identity;
global using ClassManagement.Application.Interfaces.Messaging;
global using ClassManagement.Application.Interfaces.Notifications;
global using ClassManagement.Application.Interfaces.Settings;
global using ClassManagement.Domain.Common.Interfaces;
global using ClassManagement.Domain.Modules.Admin.Entities;
global using ClassManagement.Domain.Modules.Admin.Enums;
global using ClassManagement.Domain.Modules.Assignments.Entities;
global using ClassManagement.Domain.Modules.Auth.Entities;
global using ClassManagement.Domain.Modules.Catalog.Entities;
global using ClassManagement.Domain.Modules.Classroom.Entities;
global using ClassManagement.Domain.Modules.Exams.Entities;
global using ClassManagement.Domain.Modules.Media.Entities;
global using ClassManagement.Domain.Modules.Media.Enums;
global using ClassManagement.Domain.Modules.Payment.Entities;
global using ClassManagement.Domain.Modules.Payment.Enums;
global using ClassManagement.Domain.Modules.Questions.Entities;
global using ClassManagement.Domain.Modules.Users.Entities;
global using ClassManagement.Domain.Primitives;
global using ClassManagement.Infrastructure.Identity;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
// Explicit import in each file that needs ApplicationDbContext/DatabaseSeeder to avoid
// namespace collision: ClassManagement.Infrastructure.Persistence.DbContext ≠ Microsoft.EntityFrameworkCore.DbContext
