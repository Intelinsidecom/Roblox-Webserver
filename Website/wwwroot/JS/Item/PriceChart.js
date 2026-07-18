// Item/PriceChart.js
Roblox = Roblox || {};
var green1 = "#02b757",
    gray2 = "#757575",
    gray3 = "#b8b8b8",
    white = "#fff",
    days180 = 180,
    days90 = 90,
    days30 = 30;
typeof Roblox.ItemPriceChart == "undefined" && (Roblox.ItemPriceChart = new function() {
    function n(n) {
        var t = new Date;
        return t.setDate(t.getDate() - n), t.getTime()
    }

    function t(t) {
        $(".price-chart-container").highcharts({
            chart: {
                marginLeft: 50,
                height: 130
            },
            colors: [green1],
            title: {
                text: ""
            },
            legend: {
                enabled: !1
            },
            tooltip: {
                backgroundColor: gray2,
                borderColor: gray2,
                pointFormat: '<span style="color:' + white + '">{point.y}</span>',
                headerFormat: ""
            },
            xAxis: {
                type: "datetime",
                max: +new Date,
                min: n(days180),
                tickLength: 0,
                labels: {
                    format: "{value:%m/%d}"
                }
            },
            yAxis: {
                title: {
                    text: ""
                },
                labels: {
                    formatter: function() {
                        var n = this.value;
                        return this.value >= 1e6 ? n = parseFloat(Highcharts.numberFormat(this.value / 1e6, 1)) + "M" : this.value >= 1e3 && (n = parseFloat(Highcharts.numberFormat(this.value / 1e3, 1)) + "K"), n
                    }
                }
            },
            data: {
                csv: "col,row|" + t,
                itemDelimiter: ",",
                lineDelimiter: "|"
            },
            credits: {
                enabled: !1
            }
        })
    }

    function i(t) {
        $(".price-chart-volume-container").highcharts({
            chart: {
                type: "column",
                marginLeft: 50,
                height: 50
            },
            colors: [gray3],
            title: {
                text: ""
            },
            legend: {
                enabled: !1
            },
            tooltip: {
                backgroundColor: gray2,
                borderColor: gray2,
                pointFormat: '<span style="color:' + white + '">{point.y}</span>',
                headerFormat: ""
            },
            xAxis: {
                type: "datetime",
                max: +new Date,
                min: n(days180),
                tickLength: 0,
                labels: {
                    enabled: !1
                }
            },
            yAxis: {
                gridLineWidth: 0,
                minorGridLineWidth: 0,
                title: {
                    text: ""
                },
                labels: {
                    enabled: !1
                }
            },
            plotOptions: {
                series: {
                    pointWidth: 1
                }
            },
            data: {
                csv: "col,row|" + t,
                itemDelimiter: ",",
                lineDelimiter: "|"
            },
            credits: {
                enabled: !1
            }
        })
    }

    function r() {
        Highcharts.setOptions({
            lang: {
                thousandsSep: ","
            }
        });
        var t = $(".price-chart-container").highcharts(),
            i = $(".price-chart-volume-container").highcharts();
        $("#price-chart-30-days").click(function() {
            t.xAxis[0].update({
                min: n(days30)
            }), i.xAxis[0].update({
                min: n(days30)
            })
        }), $("#price-chart-90-days").click(function() {
            t.xAxis[0].update({
                min: n(days90)
            }), i.xAxis[0].update({
                min: n(days90)
            })
        }), $("#price-chart-180-days").click(function() {
            t.xAxis[0].update({
                min: n(days180)
            }), i.xAxis[0].update({
                min: n(days180)
            })
        })
    }

    function u() {
        $(".price-chart-container").addClass("hide"), $(".price-chart-volume-container").addClass("hide"), $(".price-chart-range-dropdown").addClass("hide"), $(".price-chart-legend").addClass("hide")
    }

    function f(n) {
        $.ajax({
            url: "/asset/" + n + "/sales-data",
            type: "GET",
            async: !0,
            success: function(n) {
                if ($(".price-chart-spinner").addClass("hide"), n && n.isValid) {
                    $(".price-volume-charts-container").removeClass("hide"), n = n.data;
                    var f = $("#item-original-price"),
                        e = f.find(".text-robux");
                    $("#item-quantity-sold").text(n.QuantitySold ? Roblox.NumberFormatting.commas(n.QuantitySold) : "N/A"), n.OriginalPrice ? e.text(Roblox.NumberFormatting.commas(n.OriginalPrice)) : (f.find("#original-price-robux-icon").hide(), e.text("N/A").removeClass("text-robux").addClass("text-lead")), n.AveragePrice && $("#item-average-price").text(Roblox.NumberFormatting.commas(n.AveragePrice)), n.HundredEightyDaySalesChart === "" ? u() : (t(n.HundredEightyDaySalesChart), i(n.HundredEightyDayVolumeChart), r())
                } else $("#no-price-chart-data").removeClass("hide")
            },
            error: function() {
                $(".price-chart-spinner").addClass("hide");
                u();
            }
        })
    }
    return {
        loadPriceChart: f
    }
}), typeof Highcharts !== "undefined" && $(".price-chart-container").length && Roblox.ItemPriceChart.loadPriceChart($("#item-container").data("item-id"));