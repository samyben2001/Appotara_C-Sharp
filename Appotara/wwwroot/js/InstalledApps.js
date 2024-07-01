function disableCheckBoxes() {
	const checkboxes = document.getElementsByClassName("checkApp");
	for (var i = 0; i < checkboxes.length; i++) {
		checkboxes[i].checked = false
	}
}