using AutoMapper;
using ProjetoFinal.Application.Contracts.Dto.Activities;
using ProjetoFinal.Application.Contracts.Dto.Chat;
using ProjetoFinal.Application.Contracts.Dto.ClassGroups;
using ProjetoFinal.Application.Contracts.Dto.Contents;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Dto.Forum;
using ProjetoFinal.Application.Contracts.Dto.Media;
using ProjetoFinal.Application.Contracts.Dto.Users;
using ProjetoFinal.Domain.Entities;

namespace ProjetoFinal.Aplication.Services.AutoMapperProfiles;

public class AutoMapperProfileCadastroDto : Profile
{
    public AutoMapperProfileCadastroDto()
    {
        CreateMap<UserCreateDto, User>();
        CreateMap<UserUpdateDto, User>();

        CreateMap<CourseCategoryCreateDto, CourseCategory>();
        CreateMap<CourseCategoryUpdateDto, CourseCategory>();

        CreateMap<CourseCreateDto, Course>();
        CreateMap<CourseUpdateDto, Course>();
        CreateMap<CourseSubscriptionCreateDto, CourseSubscription>();

        CreateMap<ClassGroupCreateDto, ClassGroup>();
        CreateMap<ClassGroupUpdateDto, ClassGroup>();
        CreateMap<ClassEnrollmentRequestDto, ClassEnrollment>();

        CreateMap<CourseContentCreateDto, CourseContent>();
        CreateMap<CourseContentUpdateDto, CourseContent>();
        CreateMap<ContentAttachmentCreateDto, ContentAttachment>();

        CreateMap<ActivityCreateDto, Activity>();
        CreateMap<ActivityUpdateDto, Activity>();
        CreateMap<ActivityAttachmentCreateDto, ActivityAttachment>();

        CreateMap<SubmissionAttachmentCreateDto, SubmissionAttachment>();

        CreateMap<ForumThreadCreateDto, ForumThread>();
        CreateMap<ForumThreadUpdateDto, ForumThread>();
        CreateMap<ForumPostCreateDto, ForumPost>();
        CreateMap<ForumPostUpdateDto, ForumPost>();
        CreateMap<ForumPostAttachmentCreateDto, ForumPostAttachment>();

        CreateMap<MediaResourceCreateDto, MediaResource>();

        CreateMap<ChatMessageCreateDto, ChatMessage>();
        CreateMap<ChatMessageUpdateDto, ChatMessage>();
    }
}
