(function ($) {
    $.fn.table = function (parameters) {

        var pageIndex = 0;

        var width = 100;
        var height = 100;

        var t = this.find("table");
        if (t.length > 0) {
            width = t.width();
            height = t.height();
        }

        this.empty();
        //���ò���
        defaults = {pageSize:0};
        $.extend(defaults, parameters);


        var $table = $("<table></table>").appendTo(this);
        $table.addClass(defaults.tableClass);


    //���ݶ������ɱ���
    var $head = $("<thead><tr></tr></thead>").appendTo($table);

    if (defaults.headClass)
        $head.addClass(defaults.headClass);

    $headline = $head.find("tr");
    $.each(defaults.columns,
				function (index, head) {
				    if (!head) return;
				    var $td = $("<td>" + head.title + "</td>").appendTo($headline);
				    $td.css({ 'text-align': head.headerTextAlign });
				    if (head.titleClass)
				        $td.addClass(head.titleClass);
				}
		);


    if (typeof defaults.getDatas == "function") {
        var $tbody = $("<tbody></tbody>").appendTo($table);

        if (defaults.loading) //��ʾ�������ݵĶ���
        {
            var left = $table.position().left;
            var top = $table.position().top;


            $("#" + defaults.loading).css(
						{
						    left: left,
						    top: top,
						    width: width,
						    height: height
						}
				).show();
        }

        defaults.getDatas(function (datas) {
            $("#" + defaults.loading).hide();

            $.each(datas, function (row, data) {
                if (!data) //���Ϊ�����ݾ�����
                    return;
                var $tr = $("<tr></tr>").appendTo($tbody);
                $tr.data("data", data);
                $headline.find("td").each(
										function (column, c) {
										    var $td = $("<td></td>").appendTo($tr);
										    $td.css({ 'text-align': defaults.columns[column].textAlign });




										    if (defaults.columns[column].dataField) {
										        if (typeof defaults.onCreateCell == "function") {
										            defaults.onCreateCell($td, row, column, defaults.columns[column].dataField, data);
										        }
										        else
										            $td.html(data[defaults.columns[column].dataField]);
										    }

										    else {
										        var $button = $(format("<input type='button' command='{0}' value='{1}' />", defaults.columns[column].command, defaults.columns[column].title)).
												appendTo($td);
										        $button.click(
														function () {

														    var command = defaults.columns[column].command;
														    if (defaults.columns[column].click)
														        defaults.columns[column].click(command, data);
														}
												);
										    }
										}
								);
            }
						);

            $tbody.find("tr:odd").addClass(defaults.rowClass);
            $tbody.find("tr:even").addClass(defaults.alternateRowClass);



        }
			);


    }




    return this;
};

}
)(jQuery);