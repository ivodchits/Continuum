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
                        // Get the initial child nodes directly from the container.
                        // These are the nodes that were placed there by the C# code.
                        const initialContentNodes = Array.from(container.childNodes);
                        
                        // Clear the container. This is important because createPages will
                        // append new '.page' divs into it. We've already captured
                        // the original content that needs to be paginated.
                        container.innerHTML = ''; 
                        
                        // Create pages based on the extracted initial content
                        createPages(initialContentNodes);
                    }
                    
                    function createPages(contentNodes) {
                        pages = [];
                        let currentPageDiv = document.createElement('div');
                        currentPageDiv.className = 'page';
                        container.appendChild(currentPageDiv);
                        pages.push(currentPageDiv);
                        
                        // Maximum content height per page (e.g., 95% of page height to be safe)
                        const maxPageHeight = container.clientHeight * 0.95;
                        let currentHeight = 0;

                        // Helper function to get the rendered height of an element
                        function getElementRenderedHeight(element, parentWidthReference) {
                            if (!parentWidthReference || parentWidthReference.clientWidth === 0) {
                                // Fallback if parentWidthReference isn't ready (e.g. first page, not yet in DOM / styled)
                                // This is a rough estimate, ideally parentWidthReference is always valid.
                                // Using a default width based on CSS min-page-width (300px) - padding (20px*2)
                                console.warn("Parent width reference not available or zero, using default for measurement.");
                                // Ensure currentPageDiv is used if available, otherwise a default.
                                const effectiveParentWidth = parentWidthReference && parentWidthReference.clientWidth > 0 
                                    ? parentWidthReference.clientWidth 
                                    : parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--min-page-width') || '300');
                                
                                // Account for page padding if using default width
                                const widthForMeasurement = Math.max(50, effectiveParentWidth - (parentWidthReference === currentPageDiv ? 40 : 0) );


                                parentWidthReference = { clientWidth: widthForMeasurement };
                            }
                            
                            const clone = element.cloneNode(true); // Clone to avoid altering the original node if it's already in DOM elsewhere
                            clone.style.visibility = 'hidden';
                            clone.style.position = 'absolute'; // Use absolute positioning
                            clone.style.left = '-9999px'; // Move off-screen
                            clone.style.top = '-9999px';  // Move off-screen
                            clone.style.display = 'block'; // Ensure block display for measurement
                            
                            // Set width for measurement. If parentWidthReference is currentPageDiv, account for its padding.
                            // Otherwise, use its clientWidth directly (e.g. for tempSpan for text nodes)
                            let measureWidth = parentWidthReference.clientWidth;
                            if (parentWidthReference === currentPageDiv) {
                                measureWidth = Math.max(50, parentWidthReference.clientWidth - 40); // 20px padding L+R
                            }
                            clone.style.width = measureWidth + 'px'; 
                            
                            document.body.appendChild(clone); // Append to body for reliable computed styles
                            
                            let height = clone.offsetHeight;
                            const style = window.getComputedStyle(clone);
                            height += parseInt(style.marginTop) || 0;
                            height += parseInt(style.marginBottom) || 0;
                            
                            document.body.removeChild(clone);
                            return height;
                        }

                        contentNodes.forEach(node => {
                            const originalNodeClone = node.cloneNode(true); // This clone will be appended to the page
                            
                            // Skip empty text nodes and comments
                            if ((node.nodeType === 3 && node.textContent.trim() === '') || 
                                node.nodeType === 8) {
                                return;
                            }
                            
                            let estimatedHeight = 0;
                            if (node.nodeType === 1) { // Element node
                                const tagName = node.tagName.toLowerCase();
                                // Always start a new page for h1-h3 headings if the current page is not empty
                                if (/^h[1-3]$/i.test(tagName) && currentPageDiv.innerHTML.trim() !== '') {
                                    currentPageDiv = document.createElement('div');
                                    currentPageDiv.className = 'page';
                                    container.appendChild(currentPageDiv);
                                    pages.push(currentPageDiv);
                                    currentHeight = 0;
                                }
                                estimatedHeight = getElementRenderedHeight(originalNodeClone, currentPageDiv);
                            } else if (node.nodeType === 3) { // Text node
                                // For text nodes, wrap in a span for measurement.
                                const tempSpan = document.createElement('span');
                                // Important: use originalNodeClone here, not node.
                                // originalNodeClone is what will be appended to the page.
                                tempSpan.appendChild(originalNodeClone.cloneNode(true)); 
                                estimatedHeight = getElementRenderedHeight(tempSpan, currentPageDiv);
                            }

                            // If the current page is empty and this single element is too tall, 
                            // it will still be added (and will overflow, which is acceptable per instructions).
                            // Otherwise, if it doesn't fit, create a new page.
                            if (currentHeight + estimatedHeight > maxPageHeight && currentPageDiv.innerHTML.trim() !== '') {
                                // Create a new page
                                currentPageDiv = document.createElement('div');
                                currentPageDiv.className = 'page';
                                container.appendChild(currentPageDiv);
                                pages.push(currentPageDiv);
                                currentHeight = 0;
                            }
                            
                            // Add the original cloned node (not the one used for measurement if it was wrapped/modified)
                            currentPageDiv.appendChild(originalNodeClone);
                            currentHeight += estimatedHeight;
                        });
                        
                        // Remove empty last page if one was created due to trailing newlines or small elements
                        if (pages.length > 1 && pages[pages.length - 1].innerHTML.trim() === '') {
                            container.removeChild(pages[pages.length - 1]);
                            pages.pop();
                        }

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
            // If the HTML lacks a body tag, we can't effectively wrap its content.
            // Return a simple structure, or consider throwing an error.
            if (!html.Contains("<body")) // A simplified check
            {
                // This case might need more robust handling depending on expected inputs.
                // For now, we'll create a body with the paged content div, assuming html is just a fragment.
                return $"<body><div class=\"paged-content\">{html}</div></body>";
            }

            var bodyStartRegex = new Regex(@"<body[^>]*>", RegexOptions.IgnoreCase);
            var bodyEndRegex = new Regex(@"</body>", RegexOptions.IgnoreCase);

            var bodyStartMatch = bodyStartRegex.Match(html);
            var bodyEndMatch = bodyEndRegex.Match(html);

            if (bodyStartMatch.Success && bodyEndMatch.Success)
            {
                string bodyStartTag = bodyStartMatch.Value; // e.g., <body class="foo">
                string bodyEndTag = bodyEndMatch.Value;   // </body>

                int contentStartIndex = bodyStartMatch.Index + bodyStartTag.Length;
                int contentEndIndex = bodyEndMatch.Index;

                if (contentStartIndex <= contentEndIndex)
                {
                    string originalBodyContent = html.Substring(contentStartIndex, contentEndIndex - contentStartIndex);
                    
                    // Construct the new body with the paged-content div wrapping the original content
                    var newBodyInnerContent = $"<div class=\"paged-content\">{originalBodyContent}</div>";
                    
                    // Reconstruct the HTML:
                    // Everything before the body tag + new body start tag + new wrapped content + body end tag + everything after body tag.
                    string htmlBeforeBody = html.Substring(0, bodyStartMatch.Index);
                    string htmlAfterBody = html.Substring(bodyEndMatch.Index + bodyEndTag.Length);

                    return htmlBeforeBody + bodyStartTag + newBodyInnerContent + bodyEndTag + htmlAfterBody;
                }
            }

            // Fallback if body tags are not found as expected or content is malformed.
            // This might indicate an issue with the input HTML.
            // Wrapping the whole thing might be a last resort, but consider logging this.
            // For now, consistent with previous fallback for missing body tag:
            return $"<body><div class=\"paged-content\">{html}</div></body>";
        }
    }
}
