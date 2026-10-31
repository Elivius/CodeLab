(function () {
    // Account menu in the Site.Master header: toggle, outside click, Escape and arrow-key support.
    var button = document.getElementById('accountMenuToggle');
    var panel = document.getElementById('accountMenuPanel');
    if (button && panel) {
        var close = function (restoreFocus) {
            panel.hidden = true;
            button.setAttribute('aria-expanded', 'false');
            if (restoreFocus) { button.focus(); }
        };
        var open = function () {
            panel.hidden = false;
            button.setAttribute('aria-expanded', 'true');
        };
        button.addEventListener('click', function () { if (panel.hidden) { open(); } else { close(false); } });
        button.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowDown') { e.preventDefault(); open(); panel.querySelector('a').focus(); }
        });
        document.addEventListener('click', function (e) {
            if (!panel.contains(e.target) && !button.contains(e.target)) { close(false); }
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && !panel.hidden) { e.preventDefault(); close(true); }
        });
        document.addEventListener('focusin', function (e) {
            if (!panel.contains(e.target) && !button.contains(e.target)) { close(false); }
        });
    }

    // Back-to-top button (Site.Master and Tutor.Master): shown after scrolling 300px.
    var backToTop = document.getElementById('clBackToTop');
    if (backToTop) {
        var update = function () { backToTop.hidden = window.scrollY < 300; };
        window.addEventListener('scroll', update, { passive: true });
        window.addEventListener('pageshow', update);
        backToTop.addEventListener('click', function () {
            var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            window.scrollTo({ top: 0, behavior: reduceMotion ? 'instant' : 'smooth' });
        });
        update();
    }
}());
