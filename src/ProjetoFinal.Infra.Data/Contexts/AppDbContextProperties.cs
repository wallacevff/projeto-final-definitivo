using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;

namespace ProjetoFinal.Infra.Data.Contexts;

public partial class AppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<ClassGroup> ClassGroups => Set<ClassGroup>();
    public DbSet<ClassEnrollment> ClassEnrollments => Set<ClassEnrollment>();
    public DbSet<CourseSubscription> CourseSubscriptions => Set<CourseSubscription>();
    public DbSet<CourseContent> CourseContents => Set<CourseContent>();
    public DbSet<ContentAttachment> ContentAttachments => Set<ContentAttachment>();
    public DbSet<ContentVideoAnnotation> ContentVideoAnnotations => Set<ContentVideoAnnotation>();
    public DbSet<MediaResource> MediaResources => Set<MediaResource>();
    public DbSet<ForumThread> ForumThreads => Set<ForumThread>();
    public DbSet<ForumPost> ForumPosts => Set<ForumPost>();
    public DbSet<ForumPostAttachment> ForumPostAttachments => Set<ForumPostAttachment>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityAudience> ActivityAudiences => Set<ActivityAudience>();
    public DbSet<ActivityAttachment> ActivityAttachments => Set<ActivityAttachment>();
    public DbSet<ActivitySubmission> ActivitySubmissions => Set<ActivitySubmission>();
    public DbSet<SubmissionAttachment> SubmissionAttachments => Set<SubmissionAttachment>();
    public DbSet<VideoAnnotation> VideoAnnotations => Set<VideoAnnotation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
}
