using Continuum.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Continuum.Services
{
    public class BookReaderServiceFactory
    {
        private readonly IEnumerable<IBookReaderService> _readerServices;
        
        public BookReaderServiceFactory(IEnumerable<IBookReaderService> readerServices)
        {
            _readerServices = readerServices ?? throw new ArgumentNullException(nameof(readerServices));
        }
        
        public IBookReaderService GetReaderService(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));
                
            // Find the first reader service that can handle this book
            var readerService = _readerServices.FirstOrDefault(r => r.CanHandleBook(book));
            
            if (readerService == null)
                throw new NotSupportedException($"No reader service available for book with extension {book.FileExtension}");
                
            return readerService;
        }
    }
}
