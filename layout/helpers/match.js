var Handlebars = require('handlebars');

module.exports = function(string, regex, options){
    return (new RegExp(regex,"gi").test(string))
        ? options.fn(this)
        : options.inverse(this);
};