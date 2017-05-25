function View(jsonDocument) {
    this.Canvas = document.getElementById("Canvas");
    this.Context = this.Canvas.getContext("2d");

    var fixedDoc = "{" + jsonDocument.slice(11, jsonDocument.length);
    this.JSONDocument = JSON.parse(fixedDoc);
    console.log(this.JSONDocument);

    this.TreeHeight = 0;
    this.TreeWidth = 0;
    this.TreeOffsetX = 0;
    this.TreeOffsetY = 0;

    this._currentNodeX = 0;
    this._currentNodeY = 0;
    this._bottomScrollbarWidth = 0;
    this._sideScrollbarHeight = 0;
    this._rootObject = this.JSONDocument;
    this._lastLineWidth = 0;

    this._previousNode = null;
}

View.prototype.OnMouseDown = function() {

}

View.prototype.OnMouseUp = function() {

}

View.prototype.OnMouseMove = function() {

}

View.prototype.RenderTree = function() {
    this.Context.fillStyle = "grey";
    this.Context.fillRect(0,0, 800,600);

    this._currentX = 30;
    this._currentY = 12;

    this.Context.fillStyle = "black";
    this.Context.font = "12px arial";
    this.RenderObject(this._rootObject);
}

View.prototype.RenderObject = function(pObject) {
    for (var index in pObject) {
        if (index.includes("__proto__")) return;

        var element = pObject[index];


        if (Array.isArray(element)) {
            this._inArray = true;

            // for each `PLANT`
            for (var i = 0; i < element.length; i++) {
                console.log(index);

                this.Context.fillText(index, this._currentX, this._currentY);

                this._currentY += 12;
                this._currentX += 32;

                this.RenderObject(element[i]);

                this._currentX -= 32;
            }

            this._inArray = false;
        } else if (index.startsWith("@")) {
            console.log(index, element, "ATTRIBUTE");
        } else if (typeof(element) == "string") {
            console.log(index, element, "TEXT")

            this.Context.fillText(index, this._currentX, this._currentY);

            this._currentY += 12;
        } else {
            console.log(index, "ELEMENT");

            this.Context.fillText(index, this._currentX, this._currentY);

            this._currentX += 32;
            this._currentY += 12;

            this.RenderObject(element);
        }
    }
}