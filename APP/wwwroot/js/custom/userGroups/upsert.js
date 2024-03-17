"use strict";
let userRoles = $("#script").attr("data-userRoles").split(",");

// let adminRoles = JSON.parse($("#script").attr("data-adminRoles"));
// let salesRoles = JSON.parse($("#script").attr("data-salesRoles"));
// let userRoles = $("#script").attr("data-userRoles").split(",");
// let groupId = $("#script").attr("data-groupId");
// let admin = $("#script").attr("data-admin");
// let sales = $("#script").attr("data-sales");
// let belongsTo = $("#script").attr("data-belongsTo");

$(function () {
  app.module.controls.init();
  app.module.select2JS.load();
});

let app = {
  module: {
    controls: {
      init: function () {
        let module = app.module;
        $("#UserPermissions").select2();
      },
    },
    select2JS: {
      load: function () {
        $("#UserPermissions").select2().val(userRoles).trigger("change");
        $("#UserPermissions").select2({
          placeholder: "Select Permissions",
        });
      },
    },
  },
};
