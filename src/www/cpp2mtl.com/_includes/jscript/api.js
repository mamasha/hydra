function initialize_page(pageId, theImage) {
    $(".product-img").click(function(event) {
        handle_click(event, pageId, $(this).attr("id"));
    });

	draw_scaled_image("product-vcall", draw_vcall());
	draw_scaled_image("product-vpoint", draw_vpoint());
	draw_scaled_image("product-03", draw_hrenovina());
	draw_scaled_image("product-04", draw_hrenovina());
	draw_scaled_image("the-product", theImage);
}


function handle_click(event, pageId, clickId) {
    switch (clickId) {
        case "product-vcall":
            window.location.href = "vcall.html";
            break;
        case "product-vpoint":
            window.location.href = "vpoint.html";
            break;
        case "product-03":
            window.location.href = "hrenovina.html";
            break;
        case "product-04":
            window.location.href = "hrenovina.html";
            break;
    }

}


function draw_scaled_image(elementId, draw_image) {
//	console.log("here elementId='%s'", elementId);
	
	var paper = draw_image(elementId);
	
	var dx = paper.width;
	var dy = paper.height;
//	console.log("got paper; size {0}x{1}".format(dx, dy));
	
	var elem = $("#"+elementId);
	var width = elem.width();
	var height = elem.height();
//	console.log("container size {0}x{1}".format(width, height));
	
	paper.setSize(width, height);
	paper.setViewBox(0, 0, dx, dy);
};


String.prototype.format = function() {
    var formatted = this;
    for(arg in arguments) {
        formatted = formatted.replace("{" + arg + "}", arguments[arg]);
    }
    return formatted;
};
	