angular
    .module('$parentFolderName$.$currentFolderName$.$safeitemname$', ['ng'])
    .component('$parentFolderNameToCamel$$safeitemnameToPascal$', {
        template: '{{$safeitemname$.html}}',
        controller: $safeitemnameToPascal$Controller,
        bindings: {
            data: '<',
            onUpdate: '&'
        }

    });


$safeitemnameToPascal$Controller.$inject = ['$scope'];
function $safeitemnameToPascal$Controller($scope) {
    var ctrl = this;

    ctrl.$onInit = function () {
        //    TODO Now
    };
}