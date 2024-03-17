"use strict";
let userGroups = $("#script").attr("data-userGroups");

$(function () {
  $("#UserGroups").select2({ placeholder: "Select Groups" });
  let groups = userGroups.split(",");
  $("#UserGroups").select2().val(groups).trigger("change");
});
