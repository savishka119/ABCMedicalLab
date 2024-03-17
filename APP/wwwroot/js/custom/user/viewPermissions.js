"use strict";
let userTable;

$(function () {
  userTable = app.module.controls.load();
});

let app = {
  module: {
    controls: {
      load: function () {
        return $("#tblData").DataTable({
          order: [
            [1, "asc"],
            // [0, "asc"],
          ],
          ajax: {
            url: "/Main/User/GetAllPermissions",
          },
          scrollX: true,
          columns: [
            { data: "name", autoWidth: true },
            // { data: "belongsTo", autoWidth: true },
            { data: "description", autoWidth: true },
          ],
        });
      },
    },
  },
};
