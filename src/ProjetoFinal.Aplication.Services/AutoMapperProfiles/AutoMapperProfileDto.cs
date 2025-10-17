using System.Linq;
using AutoMapper;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Activities;
using ProjetoFinal.Application.Contracts.Dto.Chat;
using ProjetoFinal.Application.Contracts.Dto.ClassGroups;
using ProjetoFinal.Application.Contracts.Dto.Contents;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Dto.Forum;
using ProjetoFinal.Application.Contracts.Dto.Media;
using ProjetoFinal.Application.Contracts.Dto.Users;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Shared.Pagination;

namespace ProjetoFinal.Aplication.Services.AutoMapperProfiles;

public class AutoMapperProfileDto : Profile
{
    public AutoMapperProfileDto()
    {
        CreateMap(typeof(PagedResult<>), typeof(PagedResultDto<>))
            .ReverseMap()
            .PreserveReferences();

        CreateMap<PageInfo, PageInfoDto>()
            .ReverseMap();

        CreateMap<User, UserDto>().ReverseMap();

        CreateMap<Course, CourseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
            .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null ? src.Instructor.FullName : string.Empty))
            .ForMember(dest => dest.ClassGroups, opt => opt.MapFrom(src => src.ClassGroups))
            .ReverseMap();

        CreateMap<Course, CourseSummaryDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
            .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null ? src.Instructor.FullName : string.Empty));

        CreateMap<CourseSubscription, CourseSubscriptionDto>().ReverseMap();

        CreateMap<ClassGroup, ClassGroupDto>()
            .ForMember(dest => dest.ApprovedEnrollments, opt => opt.MapFrom(src => src.Enrollments.Count(e => e.Status == Domain.Enums.EnrollmentStatus.Approved)))
            .ForMember(dest => dest.PendingEnrollments, opt => opt.MapFrom(src => src.Enrollments.Count(e => e.Status == Domain.Enums.EnrollmentStatus.Pending)))
            .ForMember(dest => dest.Enrollments, opt => opt.MapFrom(src => src.Enrollments));

        CreateMap<ClassEnrollment, ClassEnrollmentDto>().ReverseMap();

        CreateMap<CourseContent, CourseContentDto>()
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ReverseMap();

        CreateMap<ContentAttachment, ContentAttachmentDto>()
            .ForMember(dest => dest.Media, opt => opt.MapFrom(src => src.MediaResource));

        CreateMap<Activity, ActivityDto>()
            .ForMember(dest => dest.Audiences, opt => opt.MapFrom(src => src.Audiences))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

        CreateMap<ActivityAudience, ActivityAudienceDto>()
            .ForMember(dest => dest.ClassGroupName, opt => opt.MapFrom(src => src.ClassGroup != null ? src.ClassGroup.Name : string.Empty));

        CreateMap<ActivityAttachment, ActivityAttachmentDto>()
            .ForMember(dest => dest.Media, opt => opt.MapFrom(src => src.MediaResource));

        CreateMap<ActivitySubmission, ActivitySubmissionDto>()
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(dest => dest.VideoAnnotations, opt => opt.MapFrom(src => src.VideoAnnotations));

        CreateMap<SubmissionAttachment, SubmissionAttachmentDto>()
            .ForMember(dest => dest.Media, opt => opt.MapFrom(src => src.MediaResource));

        CreateMap<VideoAnnotation, VideoAnnotationDto>().ReverseMap();

        CreateMap<ForumThread, ForumThreadDto>()
            .ForMember(dest => dest.Posts, opt => opt.MapFrom(src => src.Posts));

        CreateMap<ForumPost, ForumPostDto>()
            .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

        CreateMap<ForumPostAttachment, ForumPostAttachmentDto>()
            .ForMember(dest => dest.Media, opt => opt.MapFrom(src => src.MediaResource));

        CreateMap<MediaResource, MediaResourceDto>().ReverseMap();

        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(dest => dest.Media, opt => opt.MapFrom(src => src.MediaResource));
    }
}
