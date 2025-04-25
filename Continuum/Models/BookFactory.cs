using UglyToad.PdfPig;
using VersOne.Epub;

namespace Continuum.Models
{
    public class BookFactory
    {
        public static Book CreateBook(FileInfo fileInfo)
        {
            var extension = fileInfo.Extension.ToLowerInvariant();
            switch (extension)
            {
                case ".epub":
                    return CreateEPUB(fileInfo);
                case ".pdf":
                    return CreatePDF(fileInfo);
                case ".mobi":
                    return CreateMobi(fileInfo);
                case ".mp3":
                    return CreateAudiobook(fileInfo);
                default:
                    throw new NotSupportedException($"File extension '{extension}' is not supported.");
            }
        }

        static Book CreateEPUB(FileInfo fileInfo)
        {
            var epubBook = EpubReader.ReadBook(fileInfo.FullName);
            return new Book
            {
                Title = epubBook.Title,
                Author = epubBook.Author,
                CoverImage = epubBook.CoverImage ?? Array.Empty<byte>(),

                Shelf = "None",
                DateAdded = fileInfo.CreationTime,
                FilePath = fileInfo.FullName,
                FileExtension = fileInfo.Extension,
                FileSize = fileInfo.Length,
                IsAudiobook = false
            };
        }

        static Book CreatePDF(FileInfo fileInfo)
        {
            using (PdfDocument document = PdfDocument.Open(fileInfo.FullName))
            {
                var images = document.GetPage(1).GetImages();
                byte[] coverImage = images.FirstOrDefault()?.RawBytes.ToArray() ?? Array.Empty<byte>();
                return new Book
                {
                    Title = document.Information.Title ?? Path.GetFileNameWithoutExtension(fileInfo.Name),
                    Author = document.Information.Author ?? "Unknown",
                    CoverImage = coverImage,

                    Shelf = "None",
                    DateAdded = fileInfo.CreationTime,
                    FilePath = fileInfo.FullName,
                    FileExtension = fileInfo.Extension,
                    FileSize = fileInfo.Length,
                    IsAudiobook = false
                };
            }
        }

        static Book CreateMobi(FileInfo fileInfo)
        {
            return new Book
            {
                Title = Path.GetFileNameWithoutExtension(fileInfo.Name),
                Author = "Unknown",
                CoverImage = Array.Empty<byte>(), // MOBI cover extraction not implemented

                Shelf = "None",
                DateAdded = fileInfo.CreationTime,
                FilePath = fileInfo.FullName,
                FileExtension = fileInfo.Extension,
                FileSize = fileInfo.Length,
                IsAudiobook = false
            };
        }

        static Book CreateAudiobook(FileInfo fileInfo)
        {
            return new Book
            {
                Title = Path.GetFileNameWithoutExtension(fileInfo.Name),
                Author = "Unknown",
                CoverImage = Array.Empty<byte>(), // Audiobook cover extraction not implemented

                Shelf = "None",
                DateAdded = fileInfo.CreationTime,
                FilePath = fileInfo.FullName,
                FileExtension = fileInfo.Extension,
                FileSize = fileInfo.Length,
                IsAudiobook = true
            };
        }
    }
}