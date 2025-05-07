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
                    display: flex;
                    flex-direction: column;
                    flex-wrap: wrap;
                    overflow-x: auto;
                    overflow-y: hidden;
                    width: 100%;
                    height: calc(100vh - (var(--page-margin) * 2));
                    column-gap: var(--page-gap);
                    padding: var(--page-margin);
                    box-sizing: border-box;
                    scroll-behavior: smooth;
                    -webkit-overflow-scrolling: touch;
                    scrollbar-width: thin;
                    scrollbar-color: rgba(0, 0, 0, 0.2) transparent;
                }
                
                /* Custom scrollbar styling */
                .paged-content::-webkit-scrollbar {
                    height: 8px;
                }
                
                .paged-content::-webkit-scrollbar-track {
                    background: transparent;
                }
                
                .paged-content::-webkit-scrollbar-thumb {
                    background-color: rgba(0, 0, 0, 0.2);
                    border-radius: 4px;
                }
                
                /* Style for paged elements */
                .paged-content > * {
                    width: min(calc(100vw - (var(--page-margin) * 2) - var(--page-gap)), var(--max-page-width));
                    min-width: var(--min-page-width);
                    break-inside: avoid;
                    max-height: calc(100vh - (var(--page-margin) * 2));
                    overflow: hidden;
                    box-sizing: border-box;
                    padding: 0 10px;
                }
                
                /* Make sure images fit properly */
                .paged-content img {
                    max-width: 100%;
                    height: auto;
                    object-fit: contain;
                }
                
                /* Remove top margin from first element on each 'page' */
                .paged-content > *:first-child {
                    margin-top: 0;
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
            var navigationScript = @"
                function setupPagination() {
                    const container = document.querySelector('.paged-content');
                    if (!container) return;
                    
                    let currentPage = 1;
                    let totalPages = 1;
                    
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
                    
                    // Calculate pages and update info
                    calculatePages();
                    updatePageInfo();
                    
                    // Add scroll event listener to update buttons
                    container.addEventListener('scroll', handleScroll);
                    
                    // Update on resize
                    window.addEventListener('resize', function() {
                        calculatePages();
                        updatePageInfo();
                        updateButtonStates();
                    });
                    
                    function calculatePages() {
                        const containerWidth = container.scrollWidth;
                        const pageWidth = getPageWidth();
                        totalPages = Math.ceil(containerWidth / pageWidth);
                    }
                    
                    function handleScroll() {
                        const scrollPos = container.scrollLeft;
                        const pageWidth = getPageWidth();
                        currentPage = Math.floor(scrollPos / pageWidth) + 1;
                        
                        updatePageInfo();
                        updateButtonStates();
                    }
                    
                    function navigateToPrevPage() {
                        if (currentPage > 1) {
                            const pageWidth = getPageWidth();
                            container.scrollTo({
                                left: (currentPage - 2) * pageWidth,
                                behavior: 'smooth'
                            });
                        }
                    }
                    
                    function navigateToNextPage() {
                        if (currentPage < totalPages) {
                            const pageWidth = getPageWidth();
                            container.scrollTo({
                                left: currentPage * pageWidth,
                                behavior: 'smooth'
                            });
                        }
                    }
                    
                    function getPageWidth() {
                        const computedStyle = window.getComputedStyle(container);
                        const pageGap = parseInt(computedStyle.columnGap || '40', 10);
                        const pageMargin = parseInt(getComputedStyle(document.documentElement).getPropertyValue('--page-margin') || '20', 10);
                        
                        // Calculate page width including gap
                        return Math.min(window.innerWidth - (pageMargin * 2), 800) + pageGap;
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
        }

        private string WrapBodyContentInPagedContainer(string html)
        {
            // If the HTML lacks proper structure, wrap it all
            if (!html.Contains("<body"))
            {
                return $"<body><div class=\"paged-content\">{html}</div></body>";
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
                    string bodyContent = html.Substring(startIndex, endIndex - startIndex);
                    
                    // Wrap body content in paged container
                    string newBodyContent = $"<div class=\"paged-content\">{bodyContent}</div>";
                    
                    // Replace old body content with new wrapped content
                    return html.Substring(0, startIndex) + newBodyContent + html.Substring(endIndex);
                }
            }

            return html;
        }
    }
}
