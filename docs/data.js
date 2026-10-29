/* Aggregates every documentation dataset into the objects consumed by app.js.
   Load order (see index.html): legacy → guides → custom → basic → auto.generated → nav → this file. */
(function () {
    function merge(base, extra) {
        const out = Object.assign({}, base);
        Object.keys(extra || {}).forEach(function (key) {
            out[key] = Object.assign({}, out[key], extra[key]);
        });
        return out;
    }

    const merged = merge(
        merge(
            merge(
                merge(merge(merge(DOCS_LEGACY || {}, DOCS_GUIDES || {}), DOCS_EXAMPLES || {}), DOCS_WEB || {}),
                DOCS_CUSTOM || {}
            ),
            DOCS_BASIC || {}
        ),
        DOCS_ENRICH || {}
    );

    // Expose to app.js (the datasets themselves are top-level consts, not window props).
    window.DOCS_DATA = merged;
    window.DOCS_AUTO = typeof DOCS_AUTO !== "undefined" ? DOCS_AUTO : {};
    window.DOCS_NAV = typeof DOCS_NAV !== "undefined" ? DOCS_NAV : [];
})();
