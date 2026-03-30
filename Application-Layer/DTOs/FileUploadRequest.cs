namespace Application_Layer.DTOs;

public sealed record FileUploadRequest(string FileName, string ContentType, byte[] Content);
