"use strict";

cursorPagination.directive("cursorPagination", [function() {
        return {
            restrict:"A", templateUrl:"rbx-cursor-pagination", link:function(n, t, i) {
                n.cursorPaging=n[i.cursorPagination]
            }
        }
    }

    ]);