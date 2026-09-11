namespace SearchEngine.WebAPI.OpenApi.Descriptions;

public static class FileAttachmentDescriptions
{
    public const string Upload =
        """
        Mengunggah satu atau beberapa file ke storage dan menyimpan metadata setiap file ke database.

        Parameter:

        • Files
            Satu atau lebih file yang akan diunggah.

        • RecordId
            Id entity yang memiliki file.

        • Module
            Nama modul.

        Seluruh file pada satu request menggunakan Module dan RecordId yang sama.

        Contoh penggunaan:

        Module = products

        RecordId = Product.Id
                   (52f3b0c1-31d9-4766-a5d9-08ded489b1e2)
        """;

    public const string GetAll =
        """
        Mengambil metadata file attachment untuk satu record (paginated).

        Filter wajib:

        • Module

        • RecordId

        Contoh:

        Module = products

        RecordId = Product.Id
        """;

    public const string GetById =
        """
        Mengambil metadata file berdasarkan Id File Attachment.

        Endpoint ini tidak mengembalikan isi file.
        Gunakan endpoint Download untuk mengunduh file.
        """;

    public const string Download =
        """
        Mengunduh file berdasarkan Id File Attachment.
        """;

    public const string Delete =
        """
        Menghapus file beserta metadata dari storage dan database.
        """;
}
