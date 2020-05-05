///dom to svg module
function ExportarDOMCompleto() {
    var wrapper = document.getElementsByTagName("BODY")[0];
    //dom to image
    domtoimage.toSvg(wrapper).then(function (svgDataUrl) {
        //download function
        downloadPNGFromAnyImageSrc(svgDataUrl);
    });
}

function ExportarContenidoID(idElemento) {
    var wrapper = document.getElementById(idElemento);
    //dom to image
    domtoimage.toSvg(wrapper).then(function (svgDataUrl) {
        //download function
        downloadPNGFromAnyImageSrc(svgDataUrl);
    });
}

function downloadPNGFromAnyImageSrc(src) {
    //recreate the image with src recieved
    var img = new Image;
    //when image loaded (to know width and height)
    img.onload = function () {
        //drow image inside a canvas
        var canvas = convertImageToCanvas(img);
        //get image/png from convas
        var pngImage = convertCanvasToImage(canvas);
        //download
        var anchor = document.createElement('a');
        anchor.setAttribute('href', pngImage.src);
        anchor.setAttribute('download', 'image.png');
        anchor.click();
    };

    img.src = src;


    // Converts image to canvas; returns new canvas element
    function convertImageToCanvas(image) {
        var canvas = document.createElement("canvas");
        canvas.width = image.width;
        canvas.height = image.height;
        canvas.getContext("2d").drawImage(image, 0, 0);
        return canvas;
    }

    // Converts canvas to an image
    function convertCanvasToImage(canvas) {
        var image = new Image();
        image.src = canvas.toDataURL("image/png");
        return image;
    }
}

