angular
    .module('$parentFolderName$.$currentFolderName$.$safeitemname$', ['ng'])
    .component('$parentFolderNameToCamel$$safeitemnameToPascal$Page', {
        template: '{{$safeitemname$.html}}',
        controller: $safeitemnameToPascal$Controller
    });


$safeitemnameToPascal$Controller.$inject = ['$scope'];
function $safeitemnameToPascal$Controller($scope) {
    var ctrl = this;

    ctrl.$onInit = function () {
        //    TODO Now
    };
}