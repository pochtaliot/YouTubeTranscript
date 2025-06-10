window.ytFocusElement = function (element) {
    if (element && typeof element.focus === 'function') {
        setTimeout(() => {
            element.focus();
            if (typeof element.select === 'function') element.select();
        }, 0);
    }
};
