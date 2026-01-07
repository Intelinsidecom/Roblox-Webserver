// Reference/jquery.twbsPagination.js
/*!
 * jQuery pagination plugin v1.2.1
 * http://esimakin.github.io/twbs-pagination/
 *
 * Copyright 2014, Eugene Simakin
 * Released under Apache 2.0 license
 * http://apache.org/licenses/LICENSE-2.0.html
 */
(function(n, t, i, r) {
    "use strict";
    var f = n.fn.twbsPagination,
        u = function(t, i) {
            if (this.$element = n(t), this.options = n.extend({}, n.fn.twbsPagination.defaults, i), this.options.startPage < 1 || this.options.startPage > this.options.totalPages) throw new Error("Start page option is incorrect");
            if (this.options.totalPages = parseInt(this.options.totalPages), isNaN(this.options.totalPages)) throw new Error("Total pages option is not correct!");
            if (this.options.visiblePages = parseInt(this.options.visiblePages), isNaN(this.options.visiblePages)) throw new Error("Visible pages option is not correct!");
            this.options.totalPages < this.options.visiblePages && (this.options.visiblePages = this.options.totalPages), this.options.onPageClick instanceof Function && this.$element.first().bind("page", this.options.onPageClick);
            var r = typeof this.$element.prop == "function" ? this.$element.prop("tagName") : this.$element.attr("tagName");
            return this.$listContainer = r === "UL" ? this.$element : n("<ul></ul>"), this.$listContainer.addClass(this.options.paginationClass), r !== "UL" && this.$element.append(this.$listContainer), this.options.isPager ? this.renderPager(this.getPages(this.options.startPage)) : this.render(this.getPages(this.options.startPage)), this.setupEvents(), this
        };
    u.prototype = {
        constructor: u,
        destroy: function() {
            return this.$element.empty(), this.$element.removeData("twbs-pagination"), this.$element.unbind("page"), this
        },
        show: function(n) {
            if (n < 1 || n > this.options.totalPages) throw new Error("Page is incorrect.");
            return isNaN(n) ? !1 : (this.options.isPager ? this.renderPager(this.getPages(n)) : this.render(this.getPages(n)), this.setupEvents(), this.$element.trigger("page", n), this)
        },
        buildListItems: function(t) {
            var i = n(),
                u, r, f;
            for (this.options.prev && (u = t.currentPage > 1 ? t.currentPage - 1 : 1, i = i.add(this.buildItem("prev", u, this.options.prevClass))), this.options.first && (i = i.add(this.buildItem("first", 1, this.options.firstClass))), r = 0; r < t.numeric.length; r++) i = t.numeric[r] == this.options.pageEllipsis ? i.add(this.buildItem("page", t.numeric[r], this.options.notPageClass)) : i.add(this.buildItem("page", t.numeric[r], this.options.pageClass));
            return this.options.last && (i = i.add(this.buildItem("last", this.options.totalPages, this.options.lastClass))), this.options.next && (f = t.currentPage >= this.options.totalPages ? this.options.totalPages : t.currentPage + 1, i = i.add(this.buildItem("next", f, this.options.nextClass))), i
        },
        buildItem: function(t, i, r) {
            var f = n("<li></li>"),
                e = n("<a></a>"),
                u = null;
            f.addClass(r), f.data("page", i);
            switch (t) {
                case "page":
                    u = i;
                    break;
                case "first":
                    u = this.options.first;
                    break;
                case "prev":
                    u = this.options.prev;
                    break;
                case "next":
                    u = this.options.next;
                    break;
                case "last":
                    u = this.options.last
            }
            return f.append(e.attr("href", this.href(i)).html(u)), f
        },
        getPages: function(n) {
            var r = [],
                f = Math.floor(this.options.visiblePages / 2),
                t = (Math.ceil(n / this.options.visiblePages) - 1) * this.options.visiblePages + 1,
                i = Math.min(t + this.options.visiblePages - 1, this.options.totalPages),
                u;
            for (t <= 0 && (t = 1, i = this.options.visiblePages), i > this.options.totalPages && (t = this.options.totalPages - this.options.visiblePages + 1, i = this.options.totalPages), u = t; u <= i;) r.push(u), u++;
            return this.options.visiblePages < this.options.totalPages && (t > 1 && r.unshift(this.options.pageEllipsis), i + this.options.visiblePages <= this.options.totalPages && r.push(this.options.pageEllipsis)), {
                currentPage: n,
                numeric: r
            }
        },
        render: function(t) {
            this.$listContainer.children().remove(), this.$listContainer.append(this.buildListItems(t)), this.$listContainer.find("." + this.options.pageClass).removeClass(this.options.activeClass).filter(function() {
                return n(this).data("page") === t.currentPage
            }).addClass(this.options.activeClass), t.currentPage === 1 && this.$listContainer.find("." + this.options.prevClass + " a,." + this.options.firstClass + " a"), t.currentPage === this.options.totalPages && this.$listContainer.find("." + this.options.nextClass + " a,." + this.options.lastClass + " a"), this.$listContainer.find("." + this.options.firstClass).toggleClass(this.options.hideClass, t.currentPage <= this.options.visiblePages), this.$listContainer.find("." + this.options.lastClass).toggleClass(this.options.hideClass, t.currentPage > this.options.totalPages - this.options.visiblePages), this.$listContainer.find("." + this.options.prevClass).toggleClass(this.options.disabledClass, t.currentPage === 1), this.$listContainer.find("." + this.options.nextClass).toggleClass(this.options.disabledClass, t.currentPage === this.options.totalPages)
        },
        renderPager: function(t) {
            var i, u, r, f, e;
            this.$listContainer.children().remove(), i = n(), this.options.first && (i = i.add(this.buildItem("first", 1, this.options.firstClass))), this.options.prev && (u = t.currentPage > 1 ? t.currentPage - 1 : 1, i = i.add(this.buildItem("prev", u, this.options.prevClass))), r = n('<li class="pager-cur"></li>'), f = n('<span id="rbx-current-page"></span>'), r.append(f.text(t.currentPage)), i = i.add(r), r = n('<li class="pager-total"></li>'), r.append("<span>of</span><a>" + this.options.totalPages + "</a>"), i = i.add(r), this.options.next && (e = t.currentPage >= this.options.totalPages ? this.options.totalPages : t.currentPage + 1, i = i.add(this.buildItem("next", e, this.options.nextClass))), this.options.last && (i = i.add(this.buildItem("last", this.options.totalPages, this.options.lastClass))), this.$listContainer.append(i), this.$listContainer.find("." + this.options.pageClass).removeClass(this.options.activeClass).filter(function() {
                return n(this).data("page") === t.currentPage
            }).addClass(this.options.activeClass), t.currentPage === 1 && this.$listContainer.find("." + this.options.prevClass + " a,." + this.options.firstClass + " a"), t.currentPage === this.options.totalPages && this.$listContainer.find("." + this.options.nextClass + " a,." + this.options.lastClass + " a"), this.$listContainer.find("." + this.options.firstClass).toggleClass(this.options.disabledClass, t.currentPage === 1), this.$listContainer.find("." + this.options.lastClass).toggleClass(this.options.disabledClass, t.currentPage === this.options.totalPages), this.$listContainer.find("." + this.options.prevClass).toggleClass(this.options.disabledClass, t.currentPage === 1), this.$listContainer.find("." + this.options.nextClass).toggleClass(this.options.disabledClass, t.currentPage === this.options.totalPages)
        },
        setupEvents: function() {
            var t = this;
            this.$listContainer.find("li").each(function() {
                var i = n(this);
                (i.off(), i.hasClass(t.options.disabledClass) || i.hasClass(t.options.activeClass)) || i.click(function(n) {
                    n.preventDefault(), t.show(parseInt(i.data("page"), 10))
                })
            })
        },
        href: function(n) {
            return this.options.href.replace(this.options.hrefVariable, n)
        }
    }, n.fn.twbsPagination = function(t) {
        var o = Array.prototype.slice.call(arguments, 1),
            f, e = n(this),
            i = e.data("twbs-pagination"),
            s = typeof t == "object" && t;
        return i || e.data("twbs-pagination", i = new u(this, s)), typeof t == "string" && (f = i[t].apply(i, o)), f === r ? e : f
    }, n.fn.twbsPagination.defaults = {
        totalPages: 0,
        startPage: 1,
        visiblePages: 5,
        href: "#",
        hrefVariable: "{{number}}",
        first: "First",
        prev: "Previous",
        next: "Next",
        last: "Last",
        pageEllipsis: "...",
        onPageClick: null,
        paginationClass: "",
        nextClass: "pager-next",
        prevClass: "pager-prev",
        lastClass: "last",
        firstClass: "first",
        pageClass: "page",
        notPageClass: "notnumber",
        activeClass: "active",
        disabledClass: "disabled",
        hideClass: "hide",
        isPager: !1
    }, n.fn.twbsPagination.Constructor = u, n.fn.twbsPagination.noConflict = function() {
        return n.fn.twbsPagination = f, this
    }
})(jQuery, window, document);