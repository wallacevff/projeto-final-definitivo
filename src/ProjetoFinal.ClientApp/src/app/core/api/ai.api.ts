export interface AiContentSummaryDto {
  Summary: string;
  KeyPoints: string[];
  AttentionPoints: string[];
  Model: string;
  GeneratedAt: string;
}

export interface AiFrequentQuestionItemDto {
  Topic: string;
  Question: string;
  SuggestedAction: string;
  EstimatedMentions: number;
  CourseTitle: string;
  ClassGroupName: string;
}

export interface AiInstructorFrequentQuestionsDto {
  Items: AiFrequentQuestionItemDto[];
  Model: string;
  GeneratedAt: string;
}
