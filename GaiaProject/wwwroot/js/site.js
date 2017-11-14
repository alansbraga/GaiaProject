﻿// Write your Javascript code.
var hex_size = 30;
var hex_width = hex_size * Math.sqrt(3);
var hex_height = hex_size * 1.5;

//第一次运行，加载事件
var isFirstAct = true;
var isFirstAl = true;

function createMap(id,type) {
    var c = document.getElementById(id);
    var contentx = c.getContext("2d");
    console.log("js start");
    DrawMap(contentx);

    if (type == undefined || type == "build") {
        c.addEventListener('click', function (e) {
            //console.log(list);
            var xy = getEventPosition(e);
            var clickObj = getClickObj(xy.x, xy.y);
            if (clickObj.typename != undefined) {
                switch (clickObj.typename) {
                case "Mine":
                    $("#syntax").val("upgrade {0} to {1}".format(clickObj.position, "TC"));
                    break;
                case "TradeCenter":
                case "ResearchLab":
                    //$("#myModalLabel").text("新增");
                    //如果是ResearchLab
                    if (clickObj.typename == "ResearchLab") {
                        $("#updateBuildList").html('<option value="">--请选择升级建筑--</option><option value="AC1">AC1</option><option value="AC2">AC2</option>')
                        $("#sttBody").show();
                    }
                    else {
                        $("#updateBuildList").html('<option value="">--请选择升级建筑--</option><option value="RL">ResearchLab</option><option value="SH">SH</option>')
                        $("#sttBody").hide();
                    }
                    $('#myModal').modal();

                    $("#updateBuildList").change(function () {
                        //SH
                        if ($(this).val() == "SH") {
                            $("#sttBody").hide();
                        } else {
                            $("#sttBody").show();
                        }
                        //alert(2);
                    });
                    $("#stt6List").change(function () {
                        //if ($("#stt6List").val()!=)
                        $("#stt3List").val("");
                    });
                    $("#stt3List").change(function () {
                        //if ($("#stt6List").val()!=)
                        $("#stt6List").val("");
                    });

                    $("#updateQuery").click(function () {

                        var upJz = $("#updateBuildList").val();
                        if (upJz == "SH") {
                            $("#syntax").val("upgrade {0} to {1}.".format(clickObj.position,
                                upJz));
                        } else {

                            if ($("#stt6List").val() != "") {
                                $("#syntax").val("upgrade {0} to {1}.+{2}.".format(clickObj.position,
                                    upJz, $("#stt6List").val()));

                            } else {
                                $("#syntax").val("upgrade {0} to {1}.+{2}. advance {3}".format(clickObj.position, upJz, $("#stt3List").val(), $("#updatekj").val()));
                            }
                        }
                        $('#myModal').modal('hide');
                    });
                    //                        $("#updateBuildList li").click(function() {
                    //                            $("#syntax").val("upgrade {0} to {1}.+".format(clickObj.position, this.id));
                    //                            $('#myModal').modal('hide');
                    //                    });
                    break;
                //case "ResearchLab":
                //$("#syntax").val("upgrade {0} to {1}".format(clickObj.position,"AC"));
                //break;
                case "Academy":

                    break;
                case "StrongHold":
                    //alert("不能继续升级");
                    break;
                case "GaiaBuilding":
                    $("#syntax").val("build " + clickObj.position);
                    //alert("不能继续升级");
                    break;
                case "gaizao":
                    $("#syntax").val("gaia " + clickObj.position);
                    //alert("不能继续升级");
                    break;
                default:
                    console.log("不能继续升级");
                }
            } else {
                $("#syntax").val("build " + clickObj.position);
            }

            console.log(clickObj);
            //console.log("f");
            //alert(getEventPosition(e).x+"/"+getEventPosition(e).y);
        }, false);
    }
    else if (type == "act") {
        if (isFirstAct == true) {
            isFirstAct = false;
            c.addEventListener('click', function (e) {
                //console.log(list);
                var xy = getEventPosition(e);
                var clickObj = getClickObj(xy.x, xy.y);
                if (clickObj.typename != undefined) {
                    alert("不能选择已经有建筑的地点");
                } else {
                    $("#syntax").val($("#syntax").val() + ".build " + clickObj.position);
                    $('#myModalCanves').modal('hide');

                }

            }, false);
        }

    }
    else if (type == "al1" || type == "al2") {
        if (isFirstAl == true) {
            isFirstAl = false;
            //确认点事件
            $("#queryPosList").click(function() {
                var posList = "";
                $("#alPosList button").each(function () {
                    //alert($(this).text());
                    posList = posList + $(this).text()+",";
                });
                posList = posList.substring(0, posList.length - 1);
                //设置命令
                var value;
                if (type == "al1") {
                    value = "satellite " + posList
                }
                else if (type == "al2") {
                    value = "alliance " + posList
                }
                value = value + " altx " + $("#alSelectList").val();
                $("#syntax").val(value);
                //隐藏和清空
                $('#myModalCanves').modal('hide');
                $("#alPosList").html("");
            });

            c.addEventListener('click', function (e) {
                //console.log(list);
                var xy = getEventPosition(e);
                var clickObj = getClickObj(xy.x, xy.y);
                if (type == "al1" && clickObj.typename != undefined) {
                    alert("不能选择已经有建筑的地点");
                }
                if (type == "al2" && clickObj.typename == undefined) {
                    alert("不能选择空白的地点");
                } else {
                    //添加位置
                    $("#alPosList").append('<button type="button" class="btn btn-default" onclick="$(this).remove();">'+clickObj.position+'</button>');
                }

            }, false);
        }

    }


}

function DrawMap(ctx) {
    console.log("DrawMap");
    //console.log(model);
    var array = model["map"]["hexArray"];
    console.log(array);
    renderColorCycle(ctx);
    for (var i = 0; i < 20; i++) {
        for (var j = 0; j < 20; j++) {

            if (array[i][j] !== null) {
                //console.log(i, j, array[i][j].ogTerrain, ConvertIntToColor(array[i][j].ogTerrain), array[i][j].isCenter);
                DrawOneHex(ctx, j, i, ConvertIntToColor(array[i][j].tfTerrain), array[i][j].isCenter, array[i][j]);
                //console.log(i, j, array[i][j].building);
                if (array[i][j].building !== null) {
                    //建筑对象
                    buildingObj.typename = array[i][j].building.name;
                    //画房子
                    switch (array[i][j].building.name) {
                        case "Mine":
                            DrawMine(ctx, j, i, array[i][j].factionBelongTo);
                            break;
                        case "TradeCenter":
                            DrawTradingPost(ctx, j, i, array[i][j].factionBelongTo);
                            break;
                        case "ResearchLab":
                            DrawResearchLab(ctx, j, i, array[i][j].factionBelongTo);
                            break;
                        case "Academy":
                            DrawAcademy(ctx, j, i, array[i][j].factionBelongTo);
                            break;
                        case "StrongHold":
                            DrawStronghold(ctx, j, i, array[i][j].factionBelongTo);
                            break;
                        case "GaiaBuilding":
                            DrawGaiaBuilding(ctx, j, i, array[i][j].factionBelongTo);
                            break;
                        default:
                            console.log(array[i][j].building.name + "不支持");
                    }
                }
                if (array[i][j].satellite !== null) {
                    DrawSatellite(ctx, j, i, array[i][j].satellite);
                }

            }
        }
    }
}


function DrawOneHex(ctx, col, row, color, isCenter, hex) {

    //console.log(color);
    var loc = hexCenter(col, row);
    //console.log(loc[0], loc[1]);
    var name = String.fromCharCode(65 + row) + col;

    var x = loc[0] + Math.sin(Math.PI / 6) * hex_size;
    var y = loc[1] - Math.cos(Math.PI / 6) * hex_size;
    makeHexPath(ctx, x, y, hex_size, color,name);
    if (isCenter) {
        textSpaceSectorCenterName(ctx, loc[0], loc[1], hex.spaceSectorName)
    }
    textHexName(ctx, loc[0], loc[1], name , color)

}

//存储管理已经绘制的元素的一个类  
//存储的元素必须实现  
//1.创建自身路径：createPath(context);  
//2.绘制自身：drawSelf(context);  
//3.点击时的时间处理：beClick();  
var canvasobjList = [];
var MikuDrawedObjList = function () {


    this.push = function(obj) {
        canvasobjList.push(obj);
        //console.log(canvasobjList);
    };
    this.remove = function(obj) {
        for (var i = 0; i < canvasobjList.length; i++) {
            var temp = canvasobjList[i];
            if (temp === obj) {
                canvasobjList.splice(i, 1);
                return i;
            }
        }
    };
    this.getClickObj = function (context, x, y) {
        //console.log(canvasobjList);
        for (var i = 0; i < canvasobjList.length; i++) {
            if (canvasobjList[i].xmax >= x && canvasobjList[i].xmin <= x && canvasobjList[i].ymax >= y && canvasobjList[i].ymin <= y) {
                return canvasobjList[i];
            }
            //console.log(canvasobjList[i]);
        }
    };
}  
//页面元素列表  
var activeObjList = new MikuDrawedObjList();




function textHexName(ctx, row, col, name, color) {
    //console.log("textHexName" +row + col + name);
    ctx.beginPath();
    ctx.font = "10px Verdana";
    ctx.fillStyle = ConvertBackGroundColorToTextColor(color);
    ctx.fillText(name, row - 11, col + 20);
    ctx.closePath();


    //console.log({ "name": name, "row": row, "col": col });

}



function textSpaceSectorCenterName(ctx, row, col, name) {
    //console.log(row, col, "isCenter");
    ctx.beginPath();
    ctx.font = "10px Verdana";
    ctx.fillStyle = "black";
    ctx.fillText(name, row - 8, col - 12);
    ctx.closePath();
}

function hexCenter(row, col) {
    //console.log("hexCenter");
    var y_offset = 0;

    var x_offset = row % 2 ? hex_width / 2 : 0;
    var x = 5 + hex_size + col * hex_width + x_offset,
        y = 5 + hex_size + row * hex_height + y_offset;
    return [y, x];
}

var buildingObj;
function makeHexPath(ctx, x, y, size, color,name) {
    //console.log("makeHexPath",x,y,size,color);
    var angle = 0;

    ctx.beginPath();
    ctx.moveTo(x, y);

    buildingObj = { "position": name }
    for (var i = 0; i < 6; i++) {
        ctx.lineTo(x, y);

        //保存
        switch (i) {
        case 0:
            buildingObj.xmax = x;
            buildingObj.ymin = y;
            break;
        case 3:
            buildingObj.xmin = x;
            buildingObj.ymax = y;
            break;
        }


        angle += Math.PI / 3;
        x += Math.cos(angle) * size;
        y += Math.sin(angle) * size;


    }
    ctx.strokeStyle = "Black"
    ctx.fillStyle = color;
    ctx.fill();
    ctx.closePath();
    ctx.stroke();

    //添加元素
    if (color === "#D19FE8") {
        //建筑对象
        buildingObj.typename = "gaizao";
    }
    //console.log(color);
    activeObjList.push(buildingObj);
    //activeObjList.push({ "name": name, "row": row, "col": col });
}

function DrawMine(ctx, row, col,name) {
    var loc = hexCenter(row, col);

    ctx.save();

    ctx.beginPath();
    ctx.moveTo(loc[0], loc[1] - 10);
    ctx.lineTo(loc[0] + 10, loc[1]);
    ctx.lineTo(loc[0] + 10, loc[1] + 10);
    ctx.lineTo(loc[0] - 10, loc[1] + 10);
    ctx.lineTo(loc[0] - 10, loc[1]);
    ctx.closePath();

    fillBuilding(ctx, name);

    ctx.restore();
}

function DrawTradingPost(ctx, row, col, name) {
    var loc = hexCenter(row, col);

    ctx.save();

    ctx.beginPath();
    ctx.moveTo(loc[0], loc[1] - 20);
    ctx.lineTo(loc[0] + 10, loc[1] - 10);
    ctx.lineTo(loc[0] + 10, loc[1] - 3);
    ctx.lineTo(loc[0] + 20, loc[1] - 3);
    ctx.lineTo(loc[0] + 20, loc[1] + 10);
    ctx.lineTo(loc[0] - 10, loc[1] + 10);
    ctx.lineTo(loc[0] - 10, loc[1]);
    ctx.lineTo(loc[0] - 10, loc[1] - 10);
    ctx.closePath();

    fillBuilding(ctx, name);

    ctx.restore();
}

function DrawResearchLab(ctx, row, col, name) {
    var loc = hexCenter(row, col);
    loc[1] -= 5;

    ctx.save();

    ctx.beginPath();
    ctx.arc(loc[0], loc[1], 14, 0.001, Math.PI * 2, false);

    fillBuilding(ctx, name);

    ctx.restore();
}

function DrawGaiaBuilding(ctx, row, col, name) {
    var loc = hexCenter(row, col);
    loc[1] -= 15;
    loc[0] -= 10;
    ctx.save();
    fillBuilding(ctx, name);

    ctx.beginPath();
    ctx.fillRect(loc[0], loc[1], 20, 20);
    ctx.strokeRect(loc[0], loc[1], 20, 20);

    ctx.restore();
}

function DrawSatellite(ctx, row, col, satellite) {
    switch (satellite.length) {
        case 1:
            var loc = hexCenter(row, col);
            loc[1] -= 9;
            loc[0] -= 4;
            ctx.save();
            fillBuilding(ctx, satellite[0]);
            ctx.beginPath();
            ctx.fillRect(loc[0], loc[1], 8, 8);
            ctx.strokeRect(loc[0], loc[1], 8, 8);
            ctx.restore();
            break;
        case 2:
            loc = hexCenter(row, col);
            loc[1] -= 20;
            loc[0] -= 4;
            ctx.save();
            fillBuilding(ctx, satellite[0]);
            ctx.beginPath();
            ctx.fillRect(loc[0], loc[1], 8, 8);
            ctx.strokeRect(loc[0], loc[1], 8, 8);
            ctx.restore();
            loc[1] += 15;
            ctx.save();
            fillBuilding(ctx, satellite[1]);
            ctx.beginPath();
            ctx.fillRect(loc[0], loc[1], 8, 8);
            ctx.strokeRect(loc[0], loc[1], 8, 8);
            ctx.restore();
            break;
        case 3:
            loc = hexCenter(row, col);
            DrawSquare(ctx, loc[0] - 4, loc[1] - 18, satellite[0]);
            DrawSquare(ctx, loc[0] + 5, loc[1], satellite[1]);
            DrawSquare(ctx, loc[0] - 10, loc[1], satellite[2]);
            break;
        case 4:
            loc = hexCenter(row, col);
            DrawSquare(ctx, loc[0] - 10, loc[1] - 16, satellite[0]);
            DrawSquare(ctx, loc[0] + 5, loc[1] - 16, satellite[1]);
            DrawSquare(ctx, loc[0] + 5, loc[1], satellite[2]);
            DrawSquare(ctx, loc[0] - 10, loc[1], satellite[3]);
            break;
        default:
            break;
    }
}

function DrawSquare(ctx, row, col, satellite) {
    ctx.save();
    fillBuilding(ctx, satellite);
    ctx.beginPath();
    ctx.fillRect(row, col, 8, 8);
    ctx.strokeRect(row, col, 8, 8);
    ctx.restore();
}



function DrawStronghold(ctx, row, col, name) {
    var loc = hexCenter(row, col);
    loc[1] -= 5;
    var size = 15;
    var bend = 10;

    ctx.save();

    ctx.beginPath();
    ctx.moveTo(loc[0] - size, loc[1] - size);
    ctx.quadraticCurveTo(loc[0] - bend, loc[1],
        loc[0] - size, loc[1] + size);
    ctx.quadraticCurveTo(loc[0], loc[1] + bend,
        loc[0] + size, loc[1] + size);
    ctx.quadraticCurveTo(loc[0] + bend, loc[1],
        loc[0] + size, loc[1] - size);
    ctx.quadraticCurveTo(loc[0], loc[1] - bend,
        loc[0] - size, loc[1] - size);

    fillBuilding(ctx, name);

    ctx.restore();
}

function DrawAcademy(ctx, row, col, name) {
    var loc = hexCenter(row, col);
    var size = 7;
    loc[1] -= 5;

    ctx.save();

    ctx.beginPath();
    ctx.arc(loc[0] - size, loc[1], 12, Math.PI / 2, -Math.PI / 2, false);
    ctx.arc(loc[0] + size, loc[1], 12, -Math.PI / 2, Math.PI / 2, false);
    ctx.closePath();

    fillBuilding(ctx, name);

    ctx.restore();
}

function fillBuilding(ctx, name) {
    ctx.fillStyle = ConvertRaceIntToColor(name);
    ctx.fill();

    ctx.strokeStyle = "Black";
    ctx.lineWidth = 2;
    ctx.stroke();
}

function ConvertIntToColor(i) {
    //if(i==window.roundColorIndex){
    //        alert(2);   
    //}
    switch (i) {
        case 0:
            return cycle[0];
        case 1:
            return cycle[1];
        case 2:
            return cycle[2];
        case 3:
            return cycle[3];
        case 4:
            return cycle[4];
        case 5:
            return cycle[5];
        case 6:
            return cycle[6];
        case 100:
            return "#80F080";//绿色
        case 200:
            return "#D19FE8";//紫色
        case 300:
            return "#FFFFFF";
        case 400:
            return "#000000";
            // return "#d4d4d2";
        default:
            return "#FFFFFF";
    }
}
//["blue", "red", "orange", "yellow", "brown", "gray", "white"]
var cycle = ["#6bd8f3", "#f23c4d", "#ea8736", "#facd2f", "#ad5e2f", "#a3a3a3", "#d3f1f5"]; 

function ConvertRaceIntToColor(i) {
    switch (i) {
        case 0:
            return "#16a0e0";
        case 1:
            return "#16a0e0";
        case 2:
            return "#8b3a0a";
        case 3:
            return "#8b3a0a";
        case 4:
            return "#6b6868";
        case 5:
            return "#6b6868";
        case 6:
            return "#d75d0c";
        case 7:
            return "#d75d0c";
        case 8:
            return "#d71729";
        case 9:
            return "#d71729";
        case 10:
            return "#ebfafb";
        case 11:
            return "#ebfafb";
        case 12:
            return "#deb703";
        case 13:
            return "#deb703";
        default:
            return "black";


    }
}

function ConvertBackGroundColorToTextColor(color) {
    if (color === ConvertIntToColor(300)) {
        return "gray";
    } else {
        return "black";
    }
}

function renderColorCycle(ctx) {

    ctx.save()
    //ctx.scale(2, 2);
    ctx.translate(830, 50);

    var base =0;

    for (var i = 0; i < 7; ++i) {
        var terrain = cycle[(base + i) % 7];

        ctx.save()

        var size = 10;

        var angle = (Math.PI * 2 / 7) * i - Math.PI / 2;
        ctx.translate(30 * Math.cos(angle), 30 * Math.sin(angle));

        ctx.beginPath();
        ctx.arc(0, 0, size, Math.PI * 2, 0, false);
        console.log(terrain);
        ctx.fillStyle = terrain;
        ctx.fill();

        ctx.stroke();

        ctx.restore();
    }
    ctx.restore();
}
