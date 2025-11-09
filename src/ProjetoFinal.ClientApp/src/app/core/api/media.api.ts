export enum MediaKind {
  Document = 1,
  Image = 2,
  Video = 3,
  Audio = 4
}

export interface MediaResource {
  Id: string;
  FileName: string;
  OriginalFileName: string;
  ContentType: string;
  Kind: MediaKind;
  SizeInBytes: number;
  DurationInSeconds?: number;
  Width?: number;
  Height?: number;
  StoragePath: string;
  Sha256?: string;
}
