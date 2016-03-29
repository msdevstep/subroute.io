module.exports = function (grunt) {
    'use strict';
    grunt.initConfig({
        // read in the project settings from the package.json file into the pkg property
        pkg: grunt.file.readJSON('package.json'),

        copy: {
            main: {
                expand: true,
                flatten: true,
                filter: 'isFile',
                src: ["app/**/*.js", 'css/*.css'],
                dest: 'wwwroot/dist/'
            }
        },
        concat: {
            options: {
                separator: ';'
            },
            dist: {
                src: ['app/**/*.js'],
                dest: 'app/combined/subroute.js'
            }
        },
        clean: ["app/combined/"]
    });

    //Add all plugins that your project needs here
    grunt.loadNpmTasks('grunt-bowercopy');
    grunt.loadNpmTasks('grunt-karma');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-contrib-concat');
    grunt.loadNpmTasks('grunt-contrib-clean');

    grunt.registerTask('test', 'testing', function () {
        console.log('Testing sample grunt task!');
    });

    grunt.registerTask('default', ['test', 'concat:dist', 'copy:main', 'clean']);
};