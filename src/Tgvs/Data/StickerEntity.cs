namespace Tgvs.Data;

public class StickerEntity
{
    public int Id { get; set; }

    public required string VideoFileId { get; set; }

    public required string Title { get; set; }
}
