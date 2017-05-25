window.onload = load;

var view;

function load() {
    document.getElementById("file").addEventListener("change", onFileSelect, false);
}

function draw() {
    if (view !== undefined) {
        view.RenderTree();
    }
}

function onFileSelect(event) {
    var file = event.target.files[0];

    var reader = new FileReader();
    reader.onload = (function() {
        return function() {
            var contents = reader.result;
            var parser = new DOMParser();
            var xmlDoc = parser.parseFromString(contents, "text/xml");

            view = new View(xml2json(xmlDoc));
            draw();
        };
    })(reader);
    
    reader.readAsText(file);
}

function GetIndex(obj, index) {
    return obj[ Object.keys(obj)[index] ];
}

function GetLength(obj) {
    return Object.keys(obj).length;
}