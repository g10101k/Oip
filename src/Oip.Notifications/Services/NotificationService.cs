using System.Text.Json;
using System.Text.RegularExpressions;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Oip.Notifications.Base;
using Oip.Notifications.Data.Entities;
using Oip.Notifications.Data.Repositories;
using Oip.Users.Base;
using EmptyResponse = Google.Protobuf.WellKnownTypes.Empty;

namespace Oip.Notifications.Services;

/// <summary>
/// Provides notification functionality by distributing messages to all registered notification channels with error handling and logging capabilities
/// </summary>
[ApiExplorerSettings(GroupName = "grpc")]
public class NotificationService(
    ILogger<NotificationService> logger,
    ChannelService channelService,
    NotificationTypeRepository notificationTypeRepository,
    NotificationChannelRepository notificationChannelRepository,
    NotificationTemplateRepository notificationTemplateRepository,
    UserNotificationPreferenceRepository userNotificationPreferenceRepository,
    NotificationRepository notificationRepository,
    NotificationDeliveryRepository notificationDeliveryRepository,
    UserCacheRepository userRepository) : GrpcNotificationService.GrpcNotificationServiceBase
{
    /// <summary>
    /// Creates multiple notification types based on the provided requests
    /// </summary>
    /// <param name="request">The request containing a collection of notification type creation requests</param>
    /// <param name="context">The server call context</param>
    /// <returns>A response containing the results of the notification type creation operations</returns>
    /// <exception cref="RpcException">Thrown when an error occurs during notification type creation</exception>
    public override async Task<CreateNotificationTypesResponse> CreateNotificationTypes(
        CreateNotificationTypesRequest request, ServerCallContext context)
    {
        try
        {
            var results = new CreateNotificationTypesResponse();
            foreach (var notificationTypeRequest in request.Requests)
            {
                var notificationType = new NotificationTypeEntity
                {
                    Name = notificationTypeRequest.Name,
                    Description = notificationTypeRequest.Description,
                    Scope = notificationTypeRequest.Scope
                };

                var notificationTypeEntity = await notificationTypeRepository.Upsert(notificationType);
                results.NotificationType.Add(new NotificationType()
                {
                    NotificationTypeId = notificationTypeEntity.NotificationTypeId,
                    Name = notificationTypeEntity.Name,
                    Description = notificationTypeEntity.Description,
                    Scope = notificationTypeEntity.Scope
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating notification types");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create notification type"));
        }
    }

    /// <summary>
    /// Retrieves a notification type by its unique identifier
    /// </summary>
    /// <param name="request">The request containing the notification type ID</param>
    /// <param name="context">The server call context</param>
    /// <returns>A response containing the requested notification type details</returns>
    /// <exception cref="RpcException">Thrown when the notification type is not found or other RPC errors occur</exception>
    public override async Task<GetNotificationTypeResponse> GetNotificationType(GetNotificationTypeRequest request,
        ServerCallContext context)
    {
        try
        {
            var notificationType = await notificationTypeRepository.GetByIdAsync(request.NotificationTypeId);
            if (notificationType == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification type with ID {request.NotificationTypeId} not found"));
            }

            return new GetNotificationTypeResponse
            {
                NotificationType = new NotificationType
                {
                    NotificationTypeId = notificationType.NotificationTypeId,
                    Name = notificationType.Name,
                    Description = notificationType.Description,
                    Scope = notificationType.Scope
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification type: {Id}", request.NotificationTypeId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification type"));
        }
    }

    /// <summary>
    /// Updates an existing notification type with the specified details
    /// </summary>
    /// <param name="request">The request containing updated notification type information</param>
    /// <param name="context">The server call context</param>
    /// <returns>A response containing the updated notification type details</returns>
    public override async Task<UpdateNotificationTypeResponse> UpdateNotificationType(
        UpdateNotificationTypeRequest request, ServerCallContext context)
    {
        try
        {
            var notificationType = await notificationTypeRepository.GetByIdAsync(request.NotificationTypeId);
            if (notificationType == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification type with ID {request.NotificationTypeId} not found"));
            }

            notificationType.Name = request.Name;
            notificationType.Description = request.Description;
            notificationType.Scope = request.Scope;

            await notificationTypeRepository.UpdateAsync(notificationType);

            return new UpdateNotificationTypeResponse
            {
                NotificationType = new NotificationType
                {
                    NotificationTypeId = notificationType.NotificationTypeId,
                    Name = notificationType.Name,
                    Description = notificationType.Description,
                    Scope = notificationType.Scope
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating notification type: {Id}", request.NotificationTypeId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to update notification type"));
        }
    }

    /// <summary>
    /// Deletes a notification type by its identifier
    /// </summary>
    /// <param name="request">The request containing the notification type ID to delete</param>
    /// <param name="context">The server call context</param>
    /// <returns>A response indicating the success or failure of the deletion operation</returns>
    public override async Task<EmptyResponse> DeleteNotificationType(
        DeleteNotificationTypeRequest request, ServerCallContext context)
    {
        try
        {
            await notificationTypeRepository.DeleteAsync(request.NotificationTypeId);
            return new EmptyResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting notification type: {Id}", request.NotificationTypeId);
            return new EmptyResponse();
        }
    }

    /// <summary>
    /// Retrieves a list of notification types based on the provided request criteria
    /// </summary>
    /// <param name="request">The request containing filter and pagination parameters for notification types</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <return>A response containing the list of notification types and pagination information</return>
    public override async Task<ListNotificationTypesResponse> ListNotificationTypes(
        ListNotificationTypesRequest request, ServerCallContext context)
    {
        try
        {
            List<NotificationTypeEntity> notificationTypes;
            if (!string.IsNullOrEmpty(request.ScopeFilter))
            {
                notificationTypes = await notificationTypeRepository.GetByScopeAsync(request.ScopeFilter);
            }
            else
            {
                notificationTypes = await notificationTypeRepository.GetAllAsync();
            }

            var response = new ListNotificationTypesResponse();
            response.NotificationTypes.AddRange(notificationTypes.Select(nt => new NotificationType
            {
                NotificationTypeId = nt.NotificationTypeId,
                Name = nt.Name,
                Description = nt.Description,
                Scope = nt.Scope
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? notificationTypes.Count,
                TotalCount = notificationTypes.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notification types");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notification types"));
        }
    }

    /// <summary>
    /// Creates a new notification channel with the specified configuration
    /// </summary>
    /// <param name="request">The request containing channel creation parameters</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <returns>A response indicating the result of the channel creation operation</returns>
    /// <exception cref="RpcException">Thrown when there is an error during channel creation</exception>
    public override async Task<CreateNotificationChannelResponse> CreateNotificationChannel(
        CreateNotificationChannelRequest request, ServerCallContext context)
    {
        try
        {
            var notificationChannel = new NotificationChannelEntity
            {
                Code = request.Code,
                Name = request.Name,
                IsActive = request.IsActive,
                RequiresVerification = request.RequiresVerification,
                MaxRetryCount = request.MaxRetryCount
            };

            await notificationChannelRepository.AddAsync(notificationChannel);

            return new CreateNotificationChannelResponse
            {
                NotificationChannel = new NotificationChannel
                {
                    NotificationChannelId = notificationChannel.NotificationChannelId,
                    Name = notificationChannel.Name,
                    IsActive = notificationChannel.IsActive,
                    RequiresVerification = notificationChannel.RequiresVerification,
                    MaxRetryCount = notificationChannel.MaxRetryCount ?? 0
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating notification channel: {Name}", request.Name);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create notification channel"));
        }
    }

    /// <summary>
    /// Retrieves a notification channel by its unique identifier
    /// </summary>
    /// <param name="request">The request containing the notification channel ID</param>
    /// <param name="context">The server call context</param>
    /// <return>The response containing the requested notification channel details</return>
    public override async Task<GetNotificationChannelResponse> GetNotificationChannel(
        GetNotificationChannelRequest request, ServerCallContext context)
    {
        try
        {
            var notificationChannel = await notificationChannelRepository.GetByIdAsync(request.NotificationChannelId);
            if (notificationChannel == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification channel with ID {request.NotificationChannelId} not found"));
            }

            return new GetNotificationChannelResponse
            {
                NotificationChannel = new NotificationChannel
                {
                    NotificationChannelId = notificationChannel.NotificationChannelId,
                    Name = notificationChannel.Name,
                    IsActive = notificationChannel.IsActive,
                    RequiresVerification = notificationChannel.RequiresVerification,
                    MaxRetryCount = notificationChannel.MaxRetryCount ?? 0
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification channel: {Id}", request.NotificationChannelId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification channel"));
        }
    }

    /// <summary>
    /// Updates an existing notification channel with the specified settings
    /// </summary>
    /// <param name="request">The request containing updated notification channel information</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <return>Returns a response with the updated notification channel details</return>
    public override async Task<UpdateNotificationChannelResponse> UpdateNotificationChannel(
        UpdateNotificationChannelRequest request, ServerCallContext context)
    {
        try
        {
            var notificationChannel = await notificationChannelRepository.GetByIdAsync(request.NotificationChannelId);
            if (notificationChannel == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification channel with ID {request.NotificationChannelId} not found"));
            }

            notificationChannel.Name = request.Name;
            notificationChannel.IsActive = request.IsActive;
            notificationChannel.RequiresVerification = request.RequiresVerification;
            notificationChannel.MaxRetryCount = request.MaxRetryCount;

            await notificationChannelRepository.UpdateAsync(notificationChannel);

            return new UpdateNotificationChannelResponse
            {
                NotificationChannel = new NotificationChannel
                {
                    NotificationChannelId = notificationChannel.NotificationChannelId,
                    Name = notificationChannel.Name,
                    IsActive = notificationChannel.IsActive,
                    RequiresVerification = notificationChannel.RequiresVerification,
                    MaxRetryCount = notificationChannel.MaxRetryCount ?? 0
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating notification channel: {Id}", request.NotificationChannelId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to update notification channel"));
        }
    }

    /// <summary>
    /// Deletes a notification channel by its identifier
    /// </summary>
    /// <param name="request">The request containing the notification channel ID to delete</param>
    /// <param name="context">The server call context</param>
    /// <returns>A response indicating whether the deletion was successful</returns>
    public override async Task<EmptyResponse> DeleteNotificationChannel(
        DeleteNotificationChannelRequest request, ServerCallContext context)
    {
        try
        {
            await notificationChannelRepository.DeleteAsync(request.NotificationChannelId);
            return new EmptyResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting notification channel: {Id}", request.NotificationChannelId);
            return new EmptyResponse();
        }
    }

    /// <summary>
    /// Retrieves a list of notification channels based on the specified request parameters
    /// </summary>
    /// <param name="request">The request containing pagination and filtering options for notification channels</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <returns>A response containing the list of notification channels and pagination information</returns>
    public override async Task<ListNotificationChannelsResponse> ListNotificationChannels(
        ListNotificationChannelsRequest request, ServerCallContext context)
    {
        try
        {
            List<NotificationChannelEntity> notificationChannels;
            if (request.ActiveOnly)
            {
                notificationChannels = await notificationChannelRepository.GetActiveChannelsAsync();
            }
            else
            {
                notificationChannels = await notificationChannelRepository.GetAllAsync();
            }

            var response = new ListNotificationChannelsResponse();
            response.NotificationChannels.AddRange(notificationChannels.Select(nc => new NotificationChannel
            {
                NotificationChannelId = nc.NotificationChannelId,
                Name = nc.Name,
                IsActive = nc.IsActive,
                RequiresVerification = nc.RequiresVerification,
                MaxRetryCount = nc.MaxRetryCount ?? 0
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? notificationChannels.Count,
                TotalCount = notificationChannels.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notification channels");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notification channels"));
        }
    }

    /// <summary>
    /// Creates a new notification template with the specified request parameters
    /// </summary>
    /// <param name="request">The request containing notification template details</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <return>CreateNotificationTemplateResponse containing the result of the template creation</return>
    public override async Task<CreateNotificationTemplateResponse> CreateNotificationTemplate(
        CreateNotificationTemplateRequest request, ServerCallContext context)
    {
        try
        {
            var notificationTemplate = new NotificationTemplateEntity
            {
                NotificationTypeId = request.NotificationTypeId,
                SubjectTemplate = request.SubjectTemplate,
                MessageTemplate = request.MessageTemplate,
                IsActive = request.IsActive,
                NotificationTemplateChannels = request.ChannelIds.Select(x => new NotificationTemplateChannelEntity()
                {
                    NotificationChannelId = x
                }).ToList(),
                NotificationTemplateUsers = request.UsersIds.Select(x => new NotificationTemplateUserEntity()
                {
                    UserId = x
                }).ToList()
            };

            await notificationTemplateRepository.AddAsync(notificationTemplate);

            // Handle channel associations
            foreach (var channelId in request.ChannelIds)
            {
                var templateChannel = new NotificationTemplateChannelEntity
                {
                    NotificationTemplateId = notificationTemplate.NotificationTemplateId,
                    NotificationChannelId = channelId
                };
                // In a real implementation, you would save these associations
            }

            return new CreateNotificationTemplateResponse
            {
                NotificationTemplate = new NotificationTemplate
                {
                    NotificationTemplateId = notificationTemplate.NotificationTemplateId,
                    NotificationTypeId = notificationTemplate.NotificationTypeId,
                    SubjectTemplate = notificationTemplate.SubjectTemplate,
                    MessageTemplate = notificationTemplate.MessageTemplate,
                    IsActive = notificationTemplate.IsActive,
                    ChannelIds = { request.ChannelIds }
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating notification template");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create notification template"));
        }
    }

    /// <summary>
    /// Retrieves a notification template by its identifier
    /// </summary>
    /// <param name="request">The request containing the notification template identifier</param>
    /// <param name="context">The server call context</param>
    /// <return>The notification template with its associated channels</return>
    public override async Task<GetNotificationTemplateResponse> GetNotificationTemplate(
        GetNotificationTemplateRequest request, ServerCallContext context)
    {
        try
        {
            var notificationTemplate = await notificationTemplateRepository.GetWithChannelsAsync(
                request.NotificationTemplateId);

            if (notificationTemplate == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification template with ID {request.NotificationTemplateId} not found"));
            }

            return new GetNotificationTemplateResponse
            {
                NotificationTemplate = new NotificationTemplate
                {
                    NotificationTemplateId = notificationTemplate.NotificationTemplateId,
                    NotificationTypeId = notificationTemplate.NotificationTypeId,
                    SubjectTemplate = notificationTemplate.SubjectTemplate,
                    MessageTemplate = notificationTemplate.MessageTemplate,
                    IsActive = notificationTemplate.IsActive,
                    ChannelIds =
                        { notificationTemplate.NotificationTemplateChannels.Select(tc => tc.NotificationChannelId) }
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification template: {Id}", request.NotificationTemplateId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification template"));
        }
    }

    /// <summary>
    /// Updates an existing notification template with the specified request data
    /// </summary>
    /// <param name="request">The request containing updated template information</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <returns>A response indicating the result of the template update operation</returns>
    public override async Task<UpdateNotificationTemplateResponse> UpdateNotificationTemplate(
        UpdateNotificationTemplateRequest request, ServerCallContext context)
    {
        try
        {
            var notificationTemplate =
                await notificationTemplateRepository.GetByIdAsync(request.NotificationTemplateId);
            if (notificationTemplate == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification template with ID {request.NotificationTemplateId} not found"));
            }

            notificationTemplate.NotificationTypeId = request.NotificationTypeId;
            notificationTemplate.SubjectTemplate = request.SubjectTemplate;
            notificationTemplate.MessageTemplate = request.MessageTemplate;
            notificationTemplate.IsActive = request.IsActive;

            await notificationTemplateRepository.UpdateAsync(notificationTemplate);

            // In a real implementation, you would also update channel associations

            return new UpdateNotificationTemplateResponse
            {
                NotificationTemplate = new NotificationTemplate
                {
                    NotificationTemplateId = notificationTemplate.NotificationTemplateId,
                    NotificationTypeId = notificationTemplate.NotificationTypeId,
                    SubjectTemplate = notificationTemplate.SubjectTemplate,
                    MessageTemplate = notificationTemplate.MessageTemplate,
                    IsActive = notificationTemplate.IsActive,
                    ChannelIds = { request.ChannelIds }
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating notification template: {Id}", request.NotificationTemplateId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to update notification template"));
        }
    }

    /// <summary>
    /// Deletes a notification template by its identifier
    /// </summary>
    /// <param name="request">The request containing the notification template ID to delete</param>
    /// <param name="context">The server call context</param>
    /// <returns>A response indicating whether the deletion was successful</returns>
    public override async Task<EmptyResponse> DeleteNotificationTemplate(
        DeleteNotificationTemplateRequest request, ServerCallContext context)
    {
        try
        {
            await notificationTemplateRepository.DeleteAsync(request.NotificationTemplateId);
            return new EmptyResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting notification template: {Id}", request.NotificationTemplateId);
            return new EmptyResponse();
        }
    }

    /// <summary>
    /// Retrieves a list of notification templates based on the provided request parameters
    /// </summary>
    /// <param name="request">The request containing criteria for filtering notification templates</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <return>A response containing the list of notification templates matching the request criteria</return>
    public override async Task<ListNotificationTemplatesResponse> ListNotificationTemplates(
        ListNotificationTemplatesRequest request, ServerCallContext context)
    {
        try
        {
            List<NotificationTemplateEntity> notificationTemplates;
            if (request.HasNotificationTypeId)
            {
                notificationTemplates = await notificationTemplateRepository.GetByTypeWithChannelsAsync(
                    request.NotificationTypeId);
            }
            else
            {
                notificationTemplates = await notificationTemplateRepository.GetAllAsync();
            }

            // Filter by active if requested
            if (request.HasActiveOnly && request.ActiveOnly)
            {
                notificationTemplates = notificationTemplates.Where(t => t.IsActive).ToList();
            }

            var response = new ListNotificationTemplatesResponse();
            response.NotificationTemplates.AddRange(notificationTemplates.Select(nt => new NotificationTemplate
            {
                NotificationTemplateId = nt.NotificationTemplateId,
                NotificationTypeId = nt.NotificationTypeId,
                SubjectTemplate = nt.SubjectTemplate,
                MessageTemplate = nt.MessageTemplate,
                IsActive = nt.IsActive,
                ChannelIds = { nt.NotificationTemplateChannels.Select(tc => tc.NotificationChannelId) }
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? notificationTemplates.Count,
                TotalCount = notificationTemplates.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notification templates");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notification templates"));
        }
    }

    /// <summary>
    /// Sets the notification preference for a user by specifying the notification type and channel
    /// </summary>
    /// <param name="request">Request containing user ID, notification type ID, channel ID, and preference status</param>
    /// <param name="context">Server call context for the gRPC request</param>
    /// <return>Response containing the updated user notification preference details</return>
    public override async Task<SetUserNotificationPreferenceResponse> SetUserNotificationPreference(
        SetUserNotificationPreferenceRequest request, ServerCallContext context)
    {
        try
        {
            var preference = new UserNotificationPreferenceEntity
            {
                UserId = request.UserId,
                NotificationTypeId = request.NotificationTypeId,
                NotificationChannelId = request.NotificationChannelId,
                IsEnabled = request.IsEnabled
            };

            var result = await userNotificationPreferenceRepository.UpsertPreferenceAsync(preference);

            return new SetUserNotificationPreferenceResponse
            {
                Preference = new UserNotificationPreference
                {
                    UserNotificationPreferenceId = result.UserNotificationPreferenceId,
                    UserId = result.UserId,
                    NotificationTypeId = result.NotificationTypeId,
                    NotificationChannelId = result.NotificationChannelId,
                    IsEnabled = result.IsEnabled
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting user notification preference");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to set user notification preference"));
        }
    }

    /// <summary>
    /// Retrieves the notification preference for a specific user, notification type, and channel
    /// </summary>
    /// <param name="request">Request containing user ID, notification type ID, and notification channel ID</param>
    /// <param name="context">Server call context for the gRPC request</param>
    /// <return>Response containing the user's notification preference settings</return>
    public override async Task<GetUserNotificationPreferenceResponse> GetUserNotificationPreference(
        GetUserNotificationPreferenceRequest request, ServerCallContext context)
    {
        try
        {
            var preference = await userNotificationPreferenceRepository.GetPreferenceAsync(
                request.UserId, request.NotificationTypeId, request.NotificationChannelId);

            if (preference == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User notification preference not found"));
            }

            return new GetUserNotificationPreferenceResponse
            {
                Preference = new UserNotificationPreference
                {
                    UserNotificationPreferenceId = preference.UserNotificationPreferenceId,
                    UserId = preference.UserId,
                    NotificationTypeId = preference.NotificationTypeId,
                    NotificationChannelId = preference.NotificationChannelId,
                    IsEnabled = preference.IsEnabled
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user notification preference");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve user notification preference"));
        }
    }

    /// <summary>
    /// Retrieves a list of notification preferences for a specified user
    /// </summary>
    /// <param name="request">The request containing the user ID and pagination parameters</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <return>A response containing the user's notification preferences and pagination details</return>
    public override async Task<ListUserNotificationPreferencesResponse> ListUserNotificationPreferences(
        ListUserNotificationPreferencesRequest request, ServerCallContext context)
    {
        try
        {
            var preferences = await userNotificationPreferenceRepository.GetByUserIdAsync(request.UserId);

            var response = new ListUserNotificationPreferencesResponse();
            response.Preferences.AddRange(preferences.Select(p => new UserNotificationPreference
            {
                UserNotificationPreferenceId = p.UserNotificationPreferenceId,
                UserId = p.UserId,
                NotificationTypeId = p.NotificationTypeId,
                NotificationChannelId = p.NotificationChannelId,
                IsEnabled = p.IsEnabled
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? preferences.Count,
                TotalCount = preferences.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing user notification preferences");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list user notification preferences"));
        }
    }

    /// <summary>
    /// Creates a new notification with the specified type, importance, and data
    /// </summary>
    /// <param name="request">The request containing notification details</param>
    /// <param name="context">The server call context</param>
    /// <return>CreateNotificationResponse containing the created notification details</return>
    public override async Task<EmptyResponse> CreateNotification(CreateNotificationRequest request,
        ServerCallContext context)
    {
        try
        {
            var notification = new NotificationEntity
            {
                NotificationTypeId = request.NotificationTypeId,
                CreatedAt = DateTimeOffset.UtcNow,
                DataJson = request.DataJson
            };
            await notificationRepository.AddAsync(notification);

            var activeTemplates =
                await notificationTemplateRepository.GetActiveTemplatesByTypeAsync(notification.NotificationTypeId);

            if (activeTemplates.Count == 0)
            {
                logger.LogWarning("No active templates found");
            }
            else
            {
                foreach (var activeTemplate in activeTemplates)
                {
                    var messages =
                        GenerateMessagesFromTemplateAsync(request, activeTemplate, notification.NotificationId);

                    foreach (var channel in activeTemplate.NotificationTemplateChannels)
                    {
                        foreach (var userNotify in messages)
                        {
                           

                            channelService.Notify(channel.NotificationChannel.Code,  userRepository.Users[userNotify.UserId],
                                userNotify.Subject,
                                userNotify.Message);
                        }
                    }
                }
            }

            return new EmptyResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating notification");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create notification"));
        }
    }

    /// <summary>
    /// Генерирует персонализированные сообщения для каждого пользователя на основе шаблона
    /// </summary>
    private List<NotificationUserEntity> GenerateMessagesFromTemplateAsync(
        CreateNotificationRequest request,
        NotificationTemplateEntity template, long notificationId)
    {
        var userMessages = new List<NotificationUserEntity>();

        // Используем JsonDocument для парсинга без полной десериализации
        using var doc = JsonDocument.Parse(request.DataJson ?? "{}");
        var data = doc.RootElement;

        // Получаем список пользователей для этого шаблона
        var templateUsers = template.NotificationTemplateUsers;

        foreach (var user in templateUsers)
        {
            var subject = ReplaceTemplatePlaceholders(template.SubjectTemplate, data, user.UserId);
            var message = ReplaceTemplatePlaceholders(template.MessageTemplate, data, user.UserId);

            userMessages.Add(new NotificationUserEntity
            {
                NotificationId = notificationId,
                UserId = user.UserId,
                Subject = subject,
                Message = message,
            });
        }

        return userMessages;
    }


    private string ReplaceTemplatePlaceholders(string template, JsonElement data, int userId)
    {
        var result = template;
        result = result.Replace("{UserId}", userId.ToString());
        result = result.Replace("{Date}", DateTime.UtcNow.ToString("yyyy-MM-dd"));
        result = result.Replace("{Time}", DateTime.UtcNow.ToString("HH:mm:ss"));

        // Ищем все плейсхолдеры в формате {PropertyName}
        var matches = Regex.Matches(template, @"{(\w+)}");

        foreach (Match match in matches)
        {
            var propertyName = match.Groups[1].Value;

            // Игнорируем системные плейсхолдеры
            if (propertyName.Equals("UserId", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Replace(match.Value, userId.ToString());
                continue;
            }

            // Ищем свойство в JSON
            if (data.TryGetProperty(propertyName, out var propertyValue) &&
                propertyValue.ValueKind != JsonValueKind.Null)
            {
                var value = propertyValue.ValueKind switch
                {
                    JsonValueKind.String => propertyValue.GetString(),
                    JsonValueKind.Number => propertyValue.GetDecimal().ToString(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => string.Empty
                };

                result = result.Replace(match.Value, value ?? string.Empty);
            }
        }

        return result;
    }

    /// <summary>
    /// Retrieves a notification by its ID along with associated user details
    /// </summary>
    /// <param name="request">The request containing the notification ID</param>
    /// <param name="context">The server call context</param>
    /// <returns>A response containing the notification details and associated users</returns>
    public override async Task<GetNotificationResponse> GetNotification(GetNotificationRequest request,
        ServerCallContext context)
    {
        try
        {
            var notification = await notificationRepository.GetWithUsersAsync(request.NotificationId);
            if (notification == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification with ID {request.NotificationId} not found"));
            }

            var notificationUsers = notification.NotificationUsers.Select(nu => new NotificationUser
            {
                NotificationUserId = nu.NotificationUserId,
                NotificationId = nu.NotificationId,
                UserId = nu.UserId,
                Subject = nu.Subject,
                Message = nu.Message
            }).ToList();

            return new GetNotificationResponse
            {
                Notification = new Notification
                {
                    NotificationId = notification.NotificationId,
                    NotificationTypeId = notification.NotificationTypeId,
                    CreatedAt = notification.CreatedAt.ToString("o"),
                    DataJson = notification.DataJson,
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification: {Id}", request.NotificationId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification"));
        }
    }

    /// <summary>
    /// Retrieves a list of notifications based on the provided request parameters
    /// </summary>
    /// <param name="request">The request containing criteria for filtering notifications</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <return>A response containing the list of notifications matching the request criteria</return>
    public override async Task<ListNotificationsResponse> ListNotifications(ListNotificationsRequest request,
        ServerCallContext context)
    {
        try
        {
            List<NotificationEntity> notifications;

            if (request.HasUserId)
            {
                notifications = await notificationRepository.GetByUserIdAsync(request.UserId);
            }
            else if (request.HasNotificationTypeId)
            {
                notifications = await notificationRepository.GetByTypeAsync(request.NotificationTypeId);
            }
            else if (!string.IsNullOrEmpty(request.CreatedAfter))
            {
                if (DateTimeOffset.TryParse(request.CreatedAfter, out var date))
                {
                    notifications = await notificationRepository.GetCreatedAfterAsync(date);
                }
                else
                {
                    notifications = await notificationRepository.GetAllAsync();
                }
            }
            else
            {
                notifications = await notificationRepository.GetAllAsync();
            }

            var response = new ListNotificationsResponse();
            response.Notifications.AddRange(notifications.Select(n => new Notification
            {
                NotificationId = n.NotificationId,
                NotificationTypeId = n.NotificationTypeId,
                CreatedAt = n.CreatedAt.ToString("o"),
                DataJson = n.DataJson
                // NotificationUsers would be populated in a real implementation
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? notifications.Count,
                TotalCount = notifications.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notifications");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notifications"));
        }
    }

    // Notification Delivery Management
    /// <summary>
    /// Retrieves the delivery details of a specific notification by its delivery identifier
    /// </summary>
    /// <param name="request">Request containing the notification delivery identifier</param>
    /// <param name="context">Server call context for the gRPC request</param>
    /// <return>Response containing the notification delivery details</return>
    public override async Task<GetNotificationDeliveryResponse> GetNotificationDelivery(
        GetNotificationDeliveryRequest request, ServerCallContext context)
    {
        try
        {
            var delivery = await notificationDeliveryRepository.GetByIdAsync(request.NotificationDeliveryId);
            if (delivery == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Notification delivery with ID {request.NotificationDeliveryId} not found"));
            }

            return new GetNotificationDeliveryResponse
            {
                Delivery = new NotificationDelivery
                {
                    NotificationDeliveryId = delivery.NotificationDeliveryId,
                    NotificationUserId = delivery.NotificationUserId,
                    UserId = delivery.UserId,
                    NotificationChannelId = delivery.NotificationChannelId,
                    Status = (DeliveryStatus)(int)delivery.Status,
                    ExternalId = delivery.ExternalId,
                    ErrorMessage = delivery.ErrorMessage,
                    RetryCount = delivery.RetryCount,
                    CreatedAt = delivery.CreatedAt.ToString("o"),
                    SentAt = delivery.SentAt?.ToString("o"),
                    DeliveredAt = delivery.DeliveredAt?.ToString("o")
                }
            };
        }
        catch (RpcException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notification delivery: {Id}", request.NotificationDeliveryId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve notification delivery"));
        }
    }

    /// <summary>
    /// Updates the delivery status of a notification based on the provided request details
    /// </summary>
    /// <param name="request">Request containing notification delivery ID, status, external ID, and error message</param>
    /// <param name="context">Server call context for the gRPC request</param>
    /// <return>Response indicating the result of the notification delivery status update operation</return>
    public override async Task<UpdateNotificationDeliveryStatusResponse> UpdateNotificationDeliveryStatus(
        UpdateNotificationDeliveryStatusRequest request, ServerCallContext context)
    {
        try
        {
            await notificationDeliveryRepository.UpdateStatusAsync(
                request.NotificationDeliveryId,
                (Data.Enums.DeliveryStatus)(int)request.Status,
                request.ExternalId,
                request.ErrorMessage);

            var delivery = await notificationDeliveryRepository.GetByIdAsync(request.NotificationDeliveryId);

            if (delivery != null)
            {
                return new UpdateNotificationDeliveryStatusResponse
                {
                    Delivery = new NotificationDelivery
                    {
                        NotificationDeliveryId = delivery.NotificationDeliveryId,
                        NotificationUserId = delivery.NotificationUserId,
                        UserId = delivery.UserId,
                        NotificationChannelId = delivery.NotificationChannelId,
                        Status = (DeliveryStatus)(int)delivery.Status,
                        ExternalId = delivery.ExternalId,
                        ErrorMessage = delivery.ErrorMessage,
                        RetryCount = delivery.RetryCount,
                        CreatedAt = delivery.CreatedAt.ToString("o"),
                        SentAt = delivery.SentAt?.ToString("o"),
                        DeliveredAt = delivery.DeliveredAt?.ToString("o")
                    }
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating notification delivery status: {Id}", request.NotificationDeliveryId);
        }

        throw new RpcException(new Status(StatusCode.Internal, "Failed to update notification delivery status"));
    }

    /// <summary>
    /// Retrieves a list of notification deliveries based on the provided request parameters
    /// </summary>
    /// <param name="request">The request containing criteria for filtering notification deliveries</param>
    /// <param name="context">The server call context for the gRPC request</param>
    /// <return>A response containing the list of notification deliveries matching the request criteria</return>
    public override async Task<ListNotificationDeliveriesResponse> ListNotificationDeliveries(
        ListNotificationDeliveriesRequest request, ServerCallContext context)
    {
        try
        {
            List<NotificationDeliveryEntity> deliveries;

            if (request.HasUserId)
            {
                deliveries = await notificationDeliveryRepository.GetByUserIdAsync(request.UserId);
            }
            else if (request.HasNotificationChannelId)
            {
                deliveries =
                    await notificationDeliveryRepository.GetByChannelIdAsync(request.NotificationChannelId);
            }
            else if (request.HasStatusFilter)
            {
                deliveries = await notificationDeliveryRepository.GetByStatusAsync(
                    (Data.Enums.DeliveryStatus)(int)request.StatusFilter);
            }
            else
            {
                deliveries = await notificationDeliveryRepository.GetAllAsync();
            }

            var response = new ListNotificationDeliveriesResponse();
            response.Deliveries.AddRange(deliveries.Select(d => new NotificationDelivery
            {
                NotificationDeliveryId = d.NotificationDeliveryId,
                NotificationUserId = d.NotificationUserId,
                UserId = d.UserId,
                NotificationChannelId = d.NotificationChannelId,
                Status = (DeliveryStatus)(int)d.Status,
                ExternalId = d.ExternalId,
                ErrorMessage = d.ErrorMessage,
                RetryCount = d.RetryCount,
                CreatedAt = d.CreatedAt.ToString("o"),
                SentAt = d.SentAt?.ToString("o"),
                DeliveredAt = d.DeliveredAt?.ToString("o")
            }));

            // In a real implementation, you would implement proper pagination
            response.Pagination = new PaginationResponse
            {
                Page = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? deliveries.Count,
                TotalCount = deliveries.Count,
                TotalPages = 1
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing notification deliveries");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list notification deliveries"));
        }
    }

    // Event Handling
    public override async Task<HandleNotificationEventResponse> HandleNotificationEvent(
        HandleNotificationEventRequest request, ServerCallContext context)
    {
        try
        {
            // In a real implementation, you would process the event and create notifications
            // based on the event type and data
            var createdNotificationIds = new List<long>();

            // For now, we'll just return an empty response
            return new HandleNotificationEventResponse
            {
                CreatedNotificationIds = { createdNotificationIds }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling notification event: {EventType}", request.EventType);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to handle notification event"));
        }
    }
}