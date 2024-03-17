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
          ajax: {
            url: "/Main/User/GetAll",
          },
          scrollX: true,
          columns: [
            { data: "email", autoWidth: true },
            { data: "name", autoWidth: true },
            {
              data: "id",
              render: function (data, type, row) {
                $("#action .btn-success").attr(
                  "href",
                  `/Main/User/EditProfile/${data}`
                );
                $("#action .btn-primary").attr(
                  "href",
                  `/Main/User/ResetPassword/${data}`
                );
                return $("#action").clone().html();
              },
              autoWidth: true,
            },
          ],
        });
      },
    },
  },
};
