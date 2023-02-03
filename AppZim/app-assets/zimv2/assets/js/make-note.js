(function () {
    //Context menu =========== //
    const menu = document.getElementById("js-context-menu");
    const noteOption = document.getElementById("js-make-note");
    const highlightOption = document.getElementById("js-make-highlight");
    const testContainer = document.getElementById("test-container");
    const addContext = document.getElementById("js-add-context");
    const editContext = document.getElementById("js-edit-context");
    const addNote = document.getElementById("js-add-note");
    const clearHighlight = document.getElementById("js-clear-highlight");
    const clearAllHighlight = document.getElementById("js-clear-all-highlight");
    let selectionRange;
    let contextTarget;
    if (!testContainer) return;

    const toggleMenu = (command) => {
        menu.style.display = command === "show" ? "block" : "none";
    };
    const setPosition = ({ top, left }) => {
        menu.style.left = `${left}px`;
        menu.style.top = `${top}px`;
        toggleMenu("show");
    };
    const hideMenu = () => {
        toggleMenu("hide");
    };
    let wordId = 0;

    const initPopover = (el) => {
        $(el).popover({
            title: "Notes",
            trigger: "click",
            placement: "right",
            template: `<div class="popover note-popup" role="tooltip">
															<div class="header-popup">
															<a href="javascript:;" class="edit-note"><i class="far fa-window-close"></i></a>
															<h3 class="popover-header"></h3>
															</div>
															<div class="popover-body"></div>
											</div>`,
        });
        $(el).popover("show");
    };

    function getMatrix(element) {
        const values = element.style.transform.split(/\w+\(|\);?/g);
        const transform = values[1]
            .split(/,\s?/g)
            .map((item) => parseInt(item, 10));

        return {
            x: transform[0],
            y: transform[1],
            z: transform[2],
        };
    }

    const dragNote = (noteEl) => {
        let noteX = 0,
            noteY = 0,
            mouseX = 0,
            mouseY = 0;
        let header = noteEl.querySelector(".header-popup");
        const endDragNote = () => {
            document.onmouseup = null;
            document.onmousemove = null;
        };

        const calcNotePos = (e) => {
            e = e || window.event;
            e.preventDefault();
            // calculate the new cursor position:
            noteX = mouseX - e.clientX;
            noteY = mouseY - e.clientY;
            mouseX = e.clientX;
            mouseY = e.clientY;
            // set the element's new position:
            const { x, y } = getMatrix(noteEl);
            noteEl.style.transform = `translate3d(${x - noteX}px, ${
                y - noteY
            }px, 0px)`;
        };

        const startDragNote = (e) => {
            e = e || window.event;
            e.preventDefault();
            // get the mouse cursor position at startup:
            mouseX = e.clientX;
            mouseY = e.clientY;
            document.onmouseup = endDragNote;
            // call a function whenever the cursor moves:
            document.onmousemove = calcNotePos;
        };

        if (header) {
            header.onmousedown = startDragNote;
        } else {
            noteEl.onmousedown = startDragNote;
        }
    };

    const makeHighlight = (event, note) => {
        event.preventDefault();
        if (selectionRange === undefined) return;
        const span = document.createElement("span");
        span.setAttribute(
            "class",
            note === true ? "highlight-word has-note" : "highlight-word"
        );
        span.id = "word-highlight-" + wordId;
        span.dataset.content = "";
        span.setAttribute("tabindex", "0");
        if (selectionRange) {
            let range = selectionRange.cloneRange();
            let docFragment = range.cloneContents();
            if (docFragment.childElementCount > 0) return false;
            range.surroundContents(span);
            if (note === true) {
                initPopover(span);
            }
            wordId++;
        }
    };

    testContainer.addEventListener("contextmenu", (e) => {
        e.preventDefault();
        let selection =
            window.getSelection() ||
            document.getSelection() ||
            document.createRange();
        if (selection.rangeCount && selection.toString() !== "") {
            selectionRange = selection.getRangeAt(0).cloneRange();
        }
        const origin = {
            left: e.pageX,
            top: e.pageY,
        };
        editContext.style.display = "none";
        addContext.style.display = "block";
        setPosition(origin);
        return false;
    });

    $.fn.setCursorToTextEnd = function () {
        const $initialVal = this.val();
        this.val($initialVal);
    };

    function placeCaretAtEnd(el) {
        el.focus();
        if (
            typeof window.getSelection != "undefined" &&
            typeof document.createRange != "undefined"
        ) {
            const range = document.createRange();
            range.selectNodeContents(el);
            range.collapse(false);
            const sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
        } else if (typeof document.body.createTextRange != "undefined") {
            const textRange = document.body.createTextRange();
            textRange.moveToElementText(el);
            textRange.collapse(false);
            textRange.select();
        }
    }

    noteOption.addEventListener("click", (e) => makeHighlight(e, true));
    highlightOption.addEventListener("click", (e) => makeHighlight(e, false));
    document
        .getElementsByTagName("body")[0]
        .addEventListener("click", hideMenu);

    $("body").on("click", ".edit-note", function () {
        let popupId = $(this).closest(".note-popup").attr("id");
        let $spanHandle = $('[aria-describedby="' + popupId + '"]');
        $spanHandle.popover("hide");
    });

    $("body").on("focusout", ".popover-body", function () {
        try {
            let $popup = $(this).closest(".note-popup");
            let $spanHandle = $(
                '[aria-describedby="' + $popup.attr("id") + '"]'
            );
            let $popupBody = $popup.find(".popover-body");
            if ($popupBody) $spanHandle.attr("data-content", $popupBody.text());
            console.log($spanHandle);
        } catch (e) {
            console.log(e);
        }
    });

    $("body").on("inserted.bs.popover", ".has-note", function () {
        try {
            const $this = $(this);
            let $popup = $("#" + $this.attr("aria-describedby"));
            let $popupBody = $popup.find(".popover-body");
            $popupBody.attr("contenteditable", "true");
            placeCaretAtEnd($popupBody.get(0));
            dragNote($popup.get(0));
        } catch (e) {
            console.log(e);
        }
    });

    $("body").on("mouseenter", ".has-note", function () {
        let $this = $(this);
        let popupId = $this.attr("aria-describedby");
        let $popup = $(`#${popupId}`);
        $this.addClass("active");
        $popup.addClass("active");
    });
    $("body").on("mouseleave", ".has-note", function () {
        let $this = $(this);
        let popupId = $this.attr("aria-describedby");
        let $popup = $(`#${popupId}`);
        $this.removeClass("active");
        $popup.removeClass("active");
    });

    const showEditContext = (e) => {
        if (e.target.matches(".has-note")) {
            $("#js-add-note").hide();
        } else {
            $("#js-add-note").show();
        }
        const origin = {
            left: e.pageX,
            top: e.pageY,
        };
        setPosition(origin);
        editContext.style.display = "block";
        addContext.style.display = "none";
        contextTarget = e.target;
        return false;
    };
    $(testContainer).on("contextmenu", ".highlight-word", (e) =>
        showEditContext(e)
    );

    const makeNoteElement = () => {
        let target = contextTarget;
        target.classList.add("has-note");
        initPopover(target);
    };
    const _clearHighlight = () => {
        const target = contextTarget;
        const text = target.textContent;
        target.parentNode.replaceChild(document.createTextNode(text), target);
        $(target).popover("hide");
        $(target).popover("dispose");
    };

    const _clearHighlightAll = () => {
        let highlights = document.querySelectorAll(".highlight-word");
        [...highlights].map((x) => {
            let text = x.textContent;
            x.parentNode.replaceChild(document.createTextNode(text), x);
            $(x).popover("hide");
            $(x).popover("dispose");
        });
    };

    addNote.addEventListener("click", makeNoteElement);
    clearHighlight.addEventListener("click", _clearHighlight);
    clearAllHighlight.addEventListener("click", _clearHighlightAll);
})();
