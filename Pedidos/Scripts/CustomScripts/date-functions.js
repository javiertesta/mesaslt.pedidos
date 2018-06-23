/// <reference path="../jquery-3.3.1.intellisense.js" />
/// <reference path="../jquery.validate-vsdoc.js" />
/// <reference path="../jquery.validate.js" />
/// <reference path="../jquery.validate.unobtrusive.js" />

function getLocaleShortDateFormat(forDatePicker, twodigits) {
    // La funcion "replace" con la expresión regular está presente por un error en Internet Explorer que inserta ciertos caracteres unicode
    // y hace fallar la subsiguiente funcion "split".
    // Se coloca una fecha conocida con los tres parámetros distintos para
    // posteriormente poder identificar en qué posición se muestra cada uno
    // cuando se formatea como texto.
    var _str = new Date(1992, 0, 7).toLocaleDateString().replace(/[^\x00-\x7F]/g, "");
    if (twodigits) _str = _str.replace("1", "01").replace("7", "07");
    var _sep = getDateDelimiter(_str);
    function obtenerPlantilla(element, index, array) {
        switch (parseInt(element)) {
            case 1:
                if (forDatePicker) { array[index] = (element.length == 1) ? "m" : "mm" }
                else { array[index] = (element.length == 1) ? "mM" : "MM" }
                break;
            case 7:
                if (forDatePicker) { array[index] = (element.length == 1) ? "d" : "dd" }
                else { array[index] = (element.length == 1) ? "dD" : "DD" }
                break;
            case 92:
                if (forDatePicker) { array[index] = "y" }
                else { array[index] = "yy" }
                break;
            case 1992:
                if (forDatePicker) { array[index] = "yy" }
                else { array[index] = "yyyy" }
        }
    }
    var _vector = _str.split(_sep);
    _vector.some(obtenerPlantilla);
    return _vector.join(_sep);
}

function stringToDate(_date, _format, _forDatePicker) {
    var _delimiter = getDateDelimiter(_format);
    // La funcion "replace" con la expresión regular está presente por un error en Internet Explorer que inserta ciertos caracteres unicode
    // y hace fallar la subsiguiente funcion "split".
    var _formatProcessed = _format.toLowerCase().replace("dd", "d").replace("mm", "m").replace(/[^\x00-\x7F]/g, "");
    var _formatSplit = _formatProcessed.split(_delimiter);
    var _dateSplit = _date.replace(/[^\x00-\x7F]/g, "").split(_delimiter);
    var _monthIndex = _formatSplit.indexOf("m");
    var _dayIndex = _formatSplit.indexOf("d");
    if (_forDatePicker) {
        var _yearIndex = _formatSplit.indexOf("yy");
    }
    else {
        var _yearIndex = _formatSplit.indexOf("yyyy");
    }
    return new Date(parseInt(_dateSplit[_yearIndex]), parseInt(_dateSplit[_monthIndex]) - 1, parseInt(_dateSplit[_dayIndex]));
}

function checkDate(_date, _format, _allowEmpty) {
    var _delimiter = getDateDelimiter(_format);

    // Comprueba si la fecha está vacía y si contiene caracteres incorrectos.
    var cadena = "^\\d+" + _delimiter + "\\d+" + _delimiter + "\\d+$"
    var _pattern = RegExp(cadena)
    if (_date.replace(" ", "") == "" && _allowEmpty) return true;
    if (!_pattern.test(_date)) return false;

    // Separa el formato y la fecha pasada para poder trabajar.
    // La funcion "replace" con la expresión regular está presente por un error en Internet Explorer que inserta ciertos caracteres unicode
    // y hace fallar la subsiguiente funcion "split".
    var _formatProcessed = _format.toLowerCase().replace("dd", "d").replace("mm", "m").replace(/[^\x00-\x7F]/g, "");
    var _formatSplit = _formatProcessed.split(_delimiter);
    var _dateSplit = _date.replace(/[^\x00-\x7F]/g, "").split(_delimiter);
    
    // Ubica en qué posición se encuentra cada elemento.
    var _monthIndex = _formatSplit.indexOf("m");
    var _dayIndex = _formatSplit.indexOf("d");
    var _yearIndex = _formatSplit.indexOf("yyyy");
    
    // Extrae los elementos de la fecha pasada.
    var _day = parseInt(_dateSplit[_dayIndex]);
    var _month = parseInt(_dateSplit[_monthIndex]);
    var _year = parseInt(_dateSplit[_yearIndex]);

    // Comprueba el rango del año y del mes.
    if (_year < 1900 || _year > 2050) return false;
    if (_month < 1 || _month > 12) return false;

    // Analiza el rango del día.
    var _topDay;

    if (_month == 1 || _month == 3 || _month == 5 || _month == 7 || _month == 8 || _month == 10 || _month == 12) {
        _topDay = 31;
    }
    else if (_month == 4 || _month == 6 || _month == 9 || _month == 11) {
        _topDay = 30;
    }
    else if (_year % 4 == 0 ){
        _topDay = 29;
    }
    else {
        _topDay = 28;
    }

    if (_day < 1 || _day > _topDay) {
        return false;
    }
    else {
        return true;
    }
}

function getDateDelimiter(value) {
    // La funcion "replace" con la expresión regular está presente por un error en Internet Explorer que inserta ciertos caracteres unicode
    // y hace fallar la subsiguiente funcion "split".
    value.replace(/[^\x00-\x7F]/g, "");
    var _delimiter = "";
    function search(element, index, array) {
        if (isNaN(parseInt(element))) {
            // Verifica que el caracter que resultó "no numérico" sea especificamente
            // un delimitador y no un catacter de formato de fecha.
            var _cars = "ymMdDhHstT";
            if (_cars.indexOf(element) == -1) {
                _delimiter = element
                return true;
            }
        }
    }
    value.split("").some(search);
    return _delimiter;
}

function isValidDate(d) {
    if (Object.prototype.toString.call(d) !== "[object Date]")
        return false;
    return !isNaN(d.getTime());
}
