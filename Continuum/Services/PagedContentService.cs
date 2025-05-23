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
                        let significantElementsOnCurrentPage = 0; // Counter for significant elements

                        const MIN_SIGNIFICANT_ELEMENTS_PER_PAGE = 2;
                        const OVERFLOW_TOLERANCE_FACTOR = 1.10; // 10% tolerance

                        // Helper function to get the rendered height of an element
                        function getElementRenderedHeight(element, parentWidthReference) {
                            if (!parentWidthReference || parentWidthReference.clientWidth === 0) {
                                console.warn("Parent width reference not available or zero, using default for measurement.");
                                const effectiveParentWidth = parentWidthReference && parentWidthReference.clientWidth > 0 
                                    ? parentWidthReference.clientWidth 
                                    : parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--min-page-width') || '300');
                                const widthForMeasurement = Math.max(50, effectiveParentWidth - (parentWidthReference === currentPageDiv ? 40 : 0) );
                                parentWidthReference = { clientWidth: widthForMeasurement };
                            }
                            
                            const clone = element.cloneNode(true);
                            clone.style.visibility = 'hidden';
                            clone.style.position = 'absolute';
                            clone.style.left = '-9999px';
                            clone.style.top = '-9999px';
                            clone.style.display = 'block';
                            
                            let measureWidth = parentWidthReference.clientWidth;
                            if (parentWidthReference === currentPageDiv) { // Check if it is the actual page div
                                measureWidth = Math.max(50, parentWidthReference.clientWidth - 40); // Account for page padding
                            }
                            clone.style.width = measureWidth + 'px'; 
                            
                            document.body.appendChild(clone);
                            
                            let height = clone.offsetHeight;
                            const style = window.getComputedStyle(clone);
                            height += parseInt(style.marginTop) || 0;
                            height += parseInt(style.marginBottom) || 0;
                            
                            document.body.removeChild(clone);
                            return height;
                        }

                        function addElementToPage(pageDiv, elementClone, height) {
                            pageDiv.appendChild(elementClone);
                            currentHeight += height;
                            // Increment significant elements counter if the node is an element or a non-empty text node
                            if (elementClone.nodeType === 1 || (elementClone.nodeType === 3 && elementClone.textContent.trim() !== '')) {
                                significantElementsOnCurrentPage++;
                            }
                        }

                        function createNewPage() {
                            const newPage = document.createElement('div');
                            newPage.className = 'page';
                            container.appendChild(newPage);
                            pages.push(newPage);
                            currentHeight = 0;
                            significantElementsOnCurrentPage = 0;
                            return newPage;
                        }

                        // Helper function for trying to split a paragraph
                        function trySplitParagraph(paragraphNodeToSplit) {
                            const originalText = paragraphNodeToSplit.textContent || '';
                            const words = originalText.split(/\s+/).filter(word => word.length > 0);

                            if (words.length === 0) return false; // Cannot process if no words

                            let currentWordIndex = 0;
                            let isFirstChunkOfParagraph = true; 
                            // This flag tracks if we are placing the very first chunk of this original paragraph.
                            // If so, it uses the current page. Subsequent chunks must start new pages.

                            while (currentWordIndex < words.length) {
                                let paragraphChunkShell = paragraphNodeToSplit.cloneNode(false); // Fresh P shell with original styles
                                let textContentForThisChunk = "";
                                let chunkCanBePlaced = true; // Assume it can be placed until proven otherwise

                                if (!isFirstChunkOfParagraph) {
                                    currentPageDiv = createNewPage(); // Subsequent chunks start on new pages
                                }
                                
                                // Add shell to the page first to establish its context (margins etc.)
                                currentPageDiv.appendChild(paragraphChunkShell);
                                if(significantElementsOnCurrentPage === 0 && textContentForThisChunk === "") {
                                     // If page is empty and we just added an empty shell, count it once.
                                     // Or, if it's a new page, it's counted by createNewPage implicitly starting with 0, then this adds 1.
                                     // This ensures the shell itself (which has margins) is considered.
                                     // We only increment if the shell is truly the first thing *meaningfully* on this page.
                                     // The addElementToPage in the main loop usually handles this.
                                     // Let's refine: only increment sigEls when text is actually added.
                                }


                                let heightOfThisChunk = 0;
                                // Temporarily remove bottom margin for iterative height checking of text, restore it later
                                const originalBottomMargin = paragraphChunkShell.style.marginBottom;
                                paragraphChunkShell.style.marginBottom = "0px";


                                for (let i = currentWordIndex; i < words.length; i++) {
                                    const word = words[i];
                                    const prospectiveText = textContentForThisChunk + (textContentForThisChunk ? ' ' : '') + word;
                                    paragraphChunkShell.textContent = prospectiveText;
                                    
                                    let heightOfChunkWithProspectiveText = getElementRenderedHeight(paragraphChunkShell, currentPageDiv);
                                   
                                    // If current page is empty, it must accept at least one word (unless word itself is too big for any page)
                                    if (significantElementsOnCurrentPage === 0 && textContentForThisChunk === "") { // Page is empty, this is the first word for this page
                                        // No break, add first word regardless of height (it will overflow if truly massive)
                                    } else if (currentHeight + heightOfChunkWithProspectiveText > maxPageHeight) {
                                        // Word does not fit. Revert to previous text for this chunk.
                                        paragraphChunkShell.textContent = textContentForThisChunk;
                                        chunkCanBePlaced = false; // Mark that this chunk is finalized due to overflow
                                        break; 
                                    }
                                    textContentForThisChunk = prospectiveText; // Word fits, commit text
                                    currentWordIndex = i + 1; 
                                }
                                
                                paragraphChunkShell.style.marginBottom = originalBottomMargin; // Restore original bottom margin
                                heightOfThisChunk = getElementRenderedHeight(paragraphChunkShell, currentPageDiv); // Final height of this chunk

                                if (textContentForThisChunk.trim() === "") { // If chunk is empty after trying words
                                    if (paragraphChunkShell.parentNode === currentPageDiv) {
                                        currentPageDiv.removeChild(paragraphChunkShell); // Remove empty shell
                                    }
                                    // If all words were processed and the last chunk was empty, break.
                                    if (currentWordIndex >= words.length) break; 
                                    // If it was due to overflow and no text fit, it implies the next word was too big.
                                    // It will be handled by the next iteration of the while loop on a new page.
                                    if (!chunkCanBePlaced) {
                                        isFirstChunkOfParagraph = false; // Ensure next attempt is on a new page
                                        continue; // Outer while loop will make a new page
                                    }
                                } else {
                                     // Chunk has content. Update page stats.
                                    if(isFirstChunkOfParagraph || significantElementsOnCurrentPage === 0) {
                                         significantElementsOnCurrentPage++; // Count this first chunk if page was empty or this is the first.
                                    } else if (!currentPageDiv.contains(paragraphChunkShell)) {
                                        // This case should not happen if logic is correct.
                                        // It means a new page was made but shell not added.
                                        currentPageDiv.appendChild(paragraphChunkShell);
                                        significantElementsOnCurrentPage++;
                                    } else {
                                        // Shell already on page, and page wasn't empty. Increment if this is a *new* element on this page.
                                        // This is tricky because the shell is added, then text filled.
                                        // The `significantElementsOnCurrentPage` should be managed by `addElementToPage` or `createNewPage`.
                                        // The splitter is for *one* original element. It makes *one or more* new elements.
                                        // Each new element (part) should be counted once.
                                        // `createNewPage` resets sigEls to 0. `addElementToPage` increments it.
                                        // The first paragraphChunkShell is added to currentPageDiv. If currentPageDiv was empty, sigEls becomes 1.
                                        // If currentPageDiv was not empty, it's just another element.
                                        // Let's assume `addElementToPage` and `createNewPage` handle sigEls correctly.
                                        // The issue is that we are manipulating `currentPageDiv` directly here.

                                        // Revised: `significantElementsOnCurrentPage` is managed by the main loop's `addElementToPage`
                                        // and `createNewPage`. The splitter just places content.
                                        // The first chunk uses the existing `currentPageDiv`.
                                        // Subsequent chunks use `createNewPage`, which correctly sets up sigEls for the new page.
                                    }
                                    currentHeight += heightOfThisChunk;
                                }

                                if (!chunkCanBePlaced) { 
                                    isFirstChunkOfParagraph = false; 
                                } else {
                                    break; 
                                }
                            }
                            return true; // Original paragraph was processed (split or fully placed)
                        }


                        contentNodes.forEach((node, index) => {
                            const originalNodeClone = node.cloneNode(true); 
                            
                            if ((node.nodeType === 3 && node.textContent.trim() === '') || node.nodeType === 8) {
                                return; 
                            }
                            
                            let estimatedHeight = 0;
                            // For text nodes, wrap in a span for consistent height measurement as if it were an element.
                            const nodeForMeasurement = node.nodeType === 1 ? originalNodeClone : (() => {
                                const tempSpan = document.createElement('span');
                                tempSpan.appendChild(originalNodeClone.cloneNode(true));
                                return tempSpan;
                            })();
                            estimatedHeight = getElementRenderedHeight(nodeForMeasurement, currentPageDiv);


                            let forceNewPageForThisElement = false;
                            if (node.nodeType === 1 && /^h[1-3]$/i.test(node.tagName) && significantElementsOnCurrentPage > 0) {
                                forceNewPageForThisElement = true;
                            }

                            if (forceNewPageForThisElement) {
                                currentPageDiv = createNewPage();
                                addElementToPage(currentPageDiv, originalNodeClone, estimatedHeight);
                            } else {
                                const isParagraph = node.nodeType === 1 && node.tagName.toLowerCase() === 'p';
                                let paragraphWasSplitAndHandled = false;

                                if (isParagraph && significantElementsOnCurrentPage > 0) {
                                    const fullParagraphWouldOverflow = (currentHeight + estimatedHeight > maxPageHeight);
                                    const isSparselyPopulated = significantElementsOnCurrentPage < MIN_SIGNIFICANT_ELEMENTS_PER_PAGE;
                                    const canFitWithTolerance = (currentHeight + estimatedHeight <= maxPageHeight * OVERFLOW_TOLERANCE_FACTOR);

                                    if (fullParagraphWouldOverflow && !(isSparselyPopulated && canFitWithTolerance)) {
                                        // Condition where we definitely don't want to just add the full P and overflow heavily or onto a packed page.
                                        // Try to split it.
                                        if (trySplitParagraph(originalNodeClone)) {
                                            paragraphWasSplitAndHandled = true; 
                                        }
                                    }
                                }

                                if (!paragraphWasSplitAndHandled) {
                                    // Standard handling if not a paragraph, or if paragraph didn't need/couldn't be split,
                                    // or if it's the first element on a page.
                                    if (significantElementsOnCurrentPage === 0) { 
                                        addElementToPage(currentPageDiv, originalNodeClone, estimatedHeight);
                                    } else {
                                        const potentialNextHeight = currentHeight + estimatedHeight;
                                        const isPageSparselyPopulated = significantElementsOnCurrentPage < MIN_SIGNIFICANT_ELEMENTS_PER_PAGE;

                                        if (potentialNextHeight > maxPageHeight) {
                                            if (isPageSparselyPopulated && potentialNextHeight <= maxPageHeight * OVERFLOW_TOLERANCE_FACTOR) {
                                                addElementToPage(currentPageDiv, originalNodeClone, estimatedHeight);
                                            } else {
                                                currentPageDiv = createNewPage();
                                                addElementToPage(currentPageDiv, originalNodeClone, estimatedHeight);
                                            }
                                        } else {
                                            addElementToPage(currentPageDiv, originalNodeClone, estimatedHeight);
                                        }
                                    }
                                }
                            }
                        });
                        
                        if (pages.length > 0 && pages[pages.length - 1].innerHTML.trim() === '') {
                            container.removeChild(pages[pages.length - 1]);
                            pages.pop();
                        }

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
