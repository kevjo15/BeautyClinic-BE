namespace Application_Layer.DTO_s;

public sealed record FileUploadRequest(string FileName, string ContentType, byte[] Content);
