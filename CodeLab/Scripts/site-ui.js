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

    // Show/hide toggle on every password box (Login, Register, Profile, Admin user edit, ...)
    var eye = '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/></svg>';
    var eyeOff = '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M3 3l18 18M10.6 5.1A10.9 10.9 0 0 1 12 5c6.4 0 10 7 10 7a17.6 17.6 0 0 1-3.2 4.2M6.6 6.6C3.8 8.4 2 12 2 12s3.6 7 10 7a10.6 10.6 0 0 0 5.4-1.6M9.9 9.9a3 3 0 0 0 4.2 4.2"/></svg>';
    Array.prototype.forEach.call(document.querySelectorAll('input[type="password"]'), function (input) {
        var wrap = document.createElement('span');
        wrap.className = 'cl-password-wrap';
        input.parentNode.insertBefore(wrap, input);
        wrap.appendChild(input);

        var toggle = document.createElement('button');
        toggle.type = 'button';
        toggle.className = 'cl-password-toggle';
        toggle.setAttribute('aria-label', 'Show password');
        toggle.setAttribute('aria-pressed', 'false');
        toggle.innerHTML = eye;
        toggle.addEventListener('click', function () {
            var show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            toggle.innerHTML = show ? eyeOff : eye;
            toggle.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
            toggle.setAttribute('aria-pressed', show ? 'true' : 'false');
        });
        wrap.appendChild(toggle);
    });

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
