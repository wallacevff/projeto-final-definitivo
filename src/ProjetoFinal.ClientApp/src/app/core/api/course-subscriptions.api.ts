export interface CourseSubscriptionDto {
  Id: string;
  CourseId: string;
  StudentId: string;
  SubscribedAt: string;
}

export interface CourseSubscriptionCreatePayload {
  CourseId: string;
  StudentId: string;
}
