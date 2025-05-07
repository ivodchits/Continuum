using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Continuum.Services
{
    public class PagedContentService
    {
        /// <summary>
        /// Takes HTML content and modifies it to work with paged content viewing
        /// </summary>
        /// <param name="html">Original HTML content</param>
        /// <returns>Modified HTML with pagination support</returns>
        public string PreparePagedContent(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            // Add CSS for pagination to the HTML content
            var modifiedHtml = InsertPaginationStyles(html);
            
            // Wrap the body content in a paged container
            modifiedHtml = WrapBodyContentInPagedContainer(modifiedHtml);
            
            return modifiedHtml;
        }

        private string InsertPaginationStyles(string html)
        {
            var styleContent = @"
                :root {
                    --page-margin: 20px;
                    --min-page-width: 300px;
                    --min-page-height: 400px;
                    --max-page-width: 800px;
                    --page-gap: 40px;
                }
                
                body {
                    margin: 0;
                    padding: 0;
                    overflow: hidden;
                    background-color: #f5f5f5;
                    font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
                }
                  .paged-content {
                    position: relative;
                    width: 100%;
                    height: calc(100vh - (var(--page-margin) * 2));
                    padding: var(--page-margin);
                    box-sizing: border-box;
                    overflow: hidden;
                }
                
                /* Style for individual pages */
                .page {                    position: absolute;
                    top: var(--page-margin);
                    left: var(--page-margin);
                    width: min(calc(100vw - (var(--page-margin) * 2)), var(--max-page-width));
                    min-width: var(--min-page-width);
                    height: calc(100vh - (var(--page-margin) * 2));
                    overflow: hidden;
                    box-sizing: border-box;
                    padding: 0 10px;
                    background-color: white;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                    display: none; /* Hide all pages by default */
                    transition: opacity 0.2s ease;
                    opacity: 0;
                    border-radius: 4px;
                }
                
                /* Show the active page */
                .page.active {
                    display: block;
                    opacity: 1;
                }
                
                /* Page transition animations */
                .page.next-enter {
                    display: block;
                    opacity: 0;
                    transform: translateX(20px);
                    transition: opacity 0.2s ease, transform 0.2s ease;
                }
                
                .page.prev-enter {
                    display: block;
                    opacity: 0;
                    transform: translateX(-20px);
                    transition: opacity 0.2s ease, transform 0.2s ease;
                }
                
                .page.active.next-enter,
                .page.active.prev-enter {
                    opacity: 1;
                    transform: translateX(0);
                }
                
                /* Dark mode for pages */
                @media (prefers-color-scheme: dark) {
                    .page {
                        background-color: #333;
                        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
                    }
                }
                  /* Make sure images fit properly */
                .page img {
                    max-width: 100%;
                    height: auto;
                    object-fit: contain;
                }
                
                /* Remove top margin from first element on each page */
                .page > *:first-child {
                    margin-top: 0;
                }
                
                /* Content inside pages */
                .page {
                    overflow-y: hidden;
                    padding: 20px;
                    line-height: 1.6;
                }
                
                .page p {
                    margin-bottom: 1em;
                    text-align: justify;
                }
                
                .page h1, .page h2, .page h3, .page h4, .page h5, .page h6 {
                    margin-top: 0;
                    margin-bottom: 0.5em;
                    line-height: 1.2;
                }
                
                /* Tables inside pages */
                .page table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-bottom: 1em;
                }
                
                .page th, .page td {
                    padding: 8px;
                    border: 1px solid #ddd;
                }
                
                /* Navigation controls */
                .page-controls {
                    position: fixed;
                    bottom: 10px;
                    left: 0;
                    right: 0;
                    display: flex;
                    justify-content: center;
                    gap: 10px;
                    padding: 10px;
                    background-color: rgba(255, 255, 255, 0.8);
                    z-index: 1000;
                    backdrop-filter: blur(5px);
                    -webkit-backdrop-filter: blur(5px);
                    border-top: 1px solid rgba(0, 0, 0, 0.1);
                }
                
                .page-controls button {
                    padding: 8px 15px;
                    background-color: #3498db;
                    color: white;
                    border: none;
                    border-radius: 4px;
                    cursor: pointer;
                    font-size: 14px;
                    font-weight: 500;
                    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                    transition: all 0.2s ease;
                }
                
                .page-controls button:hover {
                    background-color: #2980b9;
                }
                
                .page-controls button:active {
                    transform: translateY(1px);
                }
                
                .page-controls button:disabled {
                    background-color: #cccccc;
                    cursor: not-allowed;
                    box-shadow: none;
                }
                
                .page-info {
                    padding: 8px 15px;
                    background-color: rgba(255, 255, 255, 0.8);
                    border-radius: 4px;
                    font-size: 14px;
                    color: #333;
                }
                
                /* Dark mode support */
                @media (prefers-color-scheme: dark) {
                    body {
                        background-color: #222;
                        color: #eee;
                    }
                    
                    .page-controls {
                        background-color: rgba(30, 30, 30, 0.8);
                        border-top: 1px solid rgba(255, 255, 255, 0.1);
                    }
                    
                    .page-info {
                        background-color: rgba(30, 30, 30, 0.8);
                        color: #eee;
                    }
                }
                
                /* Responsive adjustments */
                @media (max-width: 600px) {
                    :root {
                        --page-margin: 10px;
                        --min-page-width: 280px;
                    }
                    
                    .page-controls button {
                        padding: 6px 12px;
                        font-size: 12px;
                    }
                    
                    .page-info {
                        padding: 6px 12px;
                        font-size: 12px;
                    }
                }
            ";

            // Add JS for page navigation
            var navigationScript = @"                function setupPagination() {
                    const container = document.querySelector('.paged-content');
                    if (!container) return;
                    
                    let currentPage = 1;
                    let totalPages = 0;
                    let pages = [];
                    
                    // Create navigation controls
                    const controls = document.createElement('div');
                    controls.className = 'page-controls';
                    
                    const prevBtn = document.createElement('button');
                    prevBtn.textContent = '← Previous';
                    prevBtn.onclick = navigateToPrevPage;
                    
                    const pageInfo = document.createElement('div');
                    pageInfo.className = 'page-info';
                    
                    const nextBtn = document.createElement('button');
                    nextBtn.textContent = 'Next →';
                    nextBtn.onclick = navigateToNextPage;
                    
                    controls.appendChild(prevBtn);
                    controls.appendChild(pageInfo);
                    controls.appendChild(nextBtn);
                    document.body.appendChild(controls);
                    
                    // Initialize pagination
                    initializePagination();
                    
                    function initializePagination() {
                        // Process the content to create pages
                        processContent();
                        
                        // Update UI
                        updatePageInfo();
                        updateButtonStates();
                        
                        // Show the first page
                        if (pages.length > 0) {
                            pages[0].classList.add('active');
                        }
                    }
                    
                    function processContent() {
                        // Clear any existing pages
                        while (container.firstChild) {
                            container.removeChild(container.firstChild);
                        }
                        
                        // Get all content from the body
                        const content = document.body.innerHTML;
                        const tempDiv = document.createElement('div');
                        tempDiv.innerHTML = content;
                        
                        // Remove the paged-content div and controls to avoid duplication
                        const existingContainer = tempDiv.querySelector('.paged-content');
                        if (existingContainer) tempDiv.removeChild(existingContainer);
                        
                        const existingControls = tempDiv.querySelector('.page-controls');
                        if (existingControls) tempDiv.removeChild(existingControls);
                        
                        // Get all content nodes
                        const contentNodes = Array.from(tempDiv.childNodes);
                        
                        // Create pages based on content
                        createPages(contentNodes);
                    }
                    
                    function createPages(contentNodes) {
                        pages = [];
                        let currentPageDiv = document.createElement('div');
                        currentPageDiv.className = 'page';
                        container.appendChild(currentPageDiv);
                        pages.push(currentPageDiv);
                        
                        // Maximum content height per page (80% of page height to be safe)
                        const maxPageHeight = container.clientHeight * 0.8;
                        let currentHeight = 0;
                        
                        // Helper function to estimate text height
                        function estimateTextHeight(text, fontSize = 16) {
                            // Approximate chars per line
                            const charsPerLine = Math.floor(currentPageDiv.clientWidth / (fontSize * 0.5));
                            const lines = Math.ceil(text.length / charsPerLine);
                            return lines * (fontSize * 1.5); // 1.5 is a typical line height
                        }
                          // Process each node
                        contentNodes.forEach(node => {
                            // Clone the node
                            const clone = node.cloneNode(true);
                            
                            // Skip empty text nodes and comments
                            if ((node.nodeType === 3 && node.textContent.trim() === '') || 
                                node.nodeType === 8) {
                                return;
                            }
                            
                            // Special handling for headings and large blocks
                            if (node.nodeType === 1) {
                                const tagName = node.tagName.toLowerCase();
                                
                                // Always start a new page for headings (h1, h2, h3)
                                if (/^h[1-3]$/i.test(tagName) && currentPageDiv.innerHTML !== '') {
                                    currentPageDiv = document.createElement('div');
                                    currentPageDiv.className = 'page';
                                    container.appendChild(currentPageDiv);
                                    pages.push(currentPageDiv);
                                    currentHeight = 0;
                                }
                                
                                // Handle large elements that should have their own page
                                if ((tagName === 'table' && node.offsetHeight > maxPageHeight * 0.7) ||
                                    (tagName === 'pre' && node.offsetHeight > maxPageHeight * 0.7)) {
                                    // Start a new page if current isn't empty
                                    if (currentPageDiv.innerHTML !== '') {
                                        currentPageDiv = document.createElement('div');
                                        currentPageDiv.className = 'page';
                                        container.appendChild(currentPageDiv);
                                        pages.push(currentPageDiv);
                                        currentHeight = 0;
                                    }
                                    
                                    // Add the element to its own page
                                    currentPageDiv.appendChild(clone);
                                    
                                    // Start a new page for content after this element
                                    currentPageDiv = document.createElement('div');
                                    currentPageDiv.className = 'page';
                                    container.appendChild(currentPageDiv);
                                    pages.push(currentPageDiv);
                                    currentHeight = 0;
                                    
                                    return;
                                }
                                
                                // Try to split paragraphs more naturally
                                if (tagName === 'p' && node.textContent.length > 200) {
                                    const text = node.textContent;
                                    const estimatedHeight = estimateTextHeight(text);
                                    
                                    // If paragraph would go beyond page boundary and we already have content
                                    if (currentHeight + estimatedHeight > maxPageHeight && currentPageDiv.innerHTML !== '') {
                                        // Create a new page
                                        currentPageDiv = document.createElement('div');
                                        currentPageDiv.className = 'page';
                                        container.appendChild(currentPageDiv);
                                        pages.push(currentPageDiv);
                                        currentHeight = 0;
                                    }
                                    
                                    // Add the paragraph
                                    currentPageDiv.appendChild(clone);
                                    currentHeight += estimatedHeight;
                                    return;
                                }
                            }                            
                            // Estimate height for text nodes and elements
                            let estimatedHeight = 0;
                            if (node.nodeType === 3) { // Text node
                                estimatedHeight = estimateTextHeight(node.textContent);
                            } else if (node.nodeType === 1) { // Element node
                                if (node.tagName === 'IMG') {
                                    estimatedHeight = 300; // Default height for images
                                } else {
                                    estimatedHeight = estimateTextHeight(node.textContent);
                                    // Add extra height for block elements
                                    if (/^(div|p|ul|ol|table|blockquote|pre|h[1-6])$/i.test(node.tagName)) {
                                        estimatedHeight += 20; // Add margin/padding
                                    }
                                }
                            }
                            
                            // Check if adding this node would exceed the page height
                            if (currentHeight + estimatedHeight > maxPageHeight && currentPageDiv.innerHTML !== '') {
                                // Create a new page
                                currentPageDiv = document.createElement('div');
                                currentPageDiv.className = 'page';
                                container.appendChild(currentPageDiv);
                                pages.push(currentPageDiv);
                                currentHeight = 0;
                            }
                            
                            // Add the node to the current page
                            currentPageDiv.appendChild(clone);
                            currentHeight += estimatedHeight;
                        });
                        
                        // Update total pages
                        totalPages = pages.length;
                    }
                      function navigateToPrevPage() {
                        if (currentPage > 1) {
                            // Hide current page
                            const currentPageElement = pages[currentPage - 1];
                            currentPageElement.classList.remove('active');
                            
                            // Show previous page with animation
                            currentPage--;
                            const prevPageElement = pages[currentPage - 1];
                            prevPageElement.classList.add('prev-enter');
                            
                            // Trigger reflow to ensure animation works
                            void prevPageElement.offsetWidth;
                            
                            prevPageElement.classList.add('active');
                            
                            // Clean up animation classes after transition
                            setTimeout(() => {
                                prevPageElement.classList.remove('prev-enter');
                            }, 300);
                            
                            updatePageInfo();
                            updateButtonStates();
                        }
                    }
                    
                    function navigateToNextPage() {
                        if (currentPage < totalPages) {
                            // Hide current page
                            const currentPageElement = pages[currentPage - 1];
                            currentPageElement.classList.remove('active');
                            
                            // Show next page with animation
                            currentPage++;
                            const nextPageElement = pages[currentPage - 1];
                            nextPageElement.classList.add('next-enter');
                            
                            // Trigger reflow to ensure animation works
                            void nextPageElement.offsetWidth;
                            
                            nextPageElement.classList.add('active');
                            
                            // Clean up animation classes after transition
                            setTimeout(() => {
                                nextPageElement.classList.remove('next-enter');
                            }, 300);
                            
                            updatePageInfo();
                            updateButtonStates();
                        }
                    }
                    
                    function updatePageInfo() {
                        pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;
                    }
                    
                    function updateButtonStates() {
                        prevBtn.disabled = currentPage <= 1;
                        nextBtn.disabled = currentPage >= totalPages;
                    }
                    
                    // Handle keyboard navigation
                    document.addEventListener('keydown', function(e) {
                        if (e.key === 'ArrowLeft') {
                            navigateToPrevPage();
                        } else if (e.key === 'ArrowRight') {
                            navigateToNextPage();
                        }
                    });
                    
                    // Handle resize events
                    window.addEventListener('resize', function() {
                        // Reinitialize pagination on significant size changes
                        initializePagination();
                    });
                }
                
                // Initialize pagination when DOM is loaded
                if (document.readyState === 'loading') {
                    document.addEventListener('DOMContentLoaded', setupPagination);
                } else {
                    setupPagination();
                }
            ";

            // Create the style and script tags
            var styleTag = $"<style>{styleContent}</style>";
            var scriptTag = $"<script>{navigationScript}</script>";

            // Insert the style tag into the head
            if (html.Contains("<head>"))
            {
                return html.Replace("<head>", $"<head>{styleTag}{scriptTag}");
            }
            else
            {
                // If no head tag exists, add one
                return $"<!DOCTYPE html><html><head>{styleTag}{scriptTag}</head>{html}</html>";
            }
        }        private string WrapBodyContentInPagedContainer(string html)
        {
            // If the HTML lacks proper structure, wrap it all
            if (!html.Contains("<body"))
            {
                return $"<body><div class=\"paged-content\"></div></body>";
            }

            // Look for body tags to wrap content
            var bodyStartMatch = Regex.Match(html, @"<body[^>]*>");
            var bodyEndMatch = Regex.Match(html, @"</body>");

            if (bodyStartMatch.Success && bodyEndMatch.Success)
            {
                string bodyStartTag = bodyStartMatch.Value;
                
                // Get the content between body tags
                int startIndex = bodyStartMatch.Index + bodyStartTag.Length;
                int endIndex = bodyEndMatch.Index;
                
                if (startIndex < endIndex)
                {
                    // Create an empty paged container that will be filled by JavaScript
                    string newBodyContent = "<div class=\"paged-content\"></div>";
                    
                    // Replace the body content while preserving original content for JS to process
                    return html.Substring(0, startIndex) + newBodyContent + html.Substring(startIndex);
                }
            }

            return html;
        }
    }
}
