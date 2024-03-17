"use strict";
// let adminGroups = JSON.parse($("#script").attr("data-adminGroups"));
// let salesGroups = JSON.parse($("#script").attr("data-salesGroups"));
// let admin = $("#script").attr("data-admin");
// let sales = $("#script").attr("data-sales");

$(function () {
  app.module.controls.init();
});

let app = {
  module: {
    controls: {
      init: function () {
        let module = app.module;
        $("#UserGroups").select2({
          placeholder: "Select Groups",
        });
      },
    },
  },
};
