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
	