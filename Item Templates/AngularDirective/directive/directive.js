angular
    .module('$parentFolderName$.$currentFolderName$.$safeitemname$', ['ng'])
    .directive('$parentFolderNameToCamel$$safeitemnameToPascal$', $safeitemnameToPascal$Controller);

blechDirective.$inject = ['$mdTheming'];
function blechDirective($mdTheming) {
    return {
        restrict: 'E',
        controller: $safeitemnameToPascal$Controller,
        transclude: false,
        template: '{{blech.html}}',
        link: $safeitemnameToPascal$Link,
        scope: {
            data: '='
        }
    };

    function $safeitemnameToPascal$Link(scope, element, attr) {
        element.addClass('_md');
        $mdTheming(element);
    }
}

$safeitemnameToPascal$Controller.$inject = ['$scope'];
function $safeitemnameToPascal$Controller($scope) {
    var vm = {
    };
    $scope.vm = vm;


    function activate() {
    }

    activate();
}