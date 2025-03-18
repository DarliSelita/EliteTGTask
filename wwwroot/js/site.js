$(document).ready(function () {
    console.log("site.js loaded, ready to post comments.");

    let skip = 10;

    // Load more posts
    $("#loadMoreBtn").click(function () {
        $.ajax({
            url: "/Post/LoadMorePosts",
            type: "GET",
            data: { skip: skip },
            success: function (posts) {
                if (posts.length === 0) {
                    $("#loadMoreBtn").hide();
                } else {
                    posts.forEach(post => {
                        $("#postContainer").append(`
                            <div class="card mb-3">
                                <div class="card-body">
                                    <h5 class="card-title">${post.title}</h5>
                                    <h6 class="card-subtitle text-muted">By ${post.author} on ${post.createdDate}</h6>
                                    <p class="card-text">${post.description}</p>
                                </div>
                            </div>
                        `);
                    });
                    skip += 10;
                }
            }
        });
    });

    // Event listener for the "Post Comment" button
    $(document).on("click", ".post-comment", function () {
        var postId = $(this).data("post-id");
        var commentText = $(this).siblings("textarea.comment-text").val();

        if (commentText.trim() === "") {
            alert("Comment cannot be empty!");
            return;
        }

        $.ajax({
            url: "/Comment/AddComment",
            type: "POST",
            data: { postId: postId, text: commentText },
            success: function (response) {
                // Update the UI with the new comment
                var newCommentHtml = `
                    <div class="card my-2" id="comment-${response.commentId}">
                        <div class="card-body">
                            <h6 class="card-subtitle text-muted">${response.username}</h6>
                            <p class="card-text">${response.text}</p>
                            <button class="btn btn-sm btn-warning edit-comment" data-comment-id="${response.commentId}">Edit</button>
                        </div>
                    </div>
                `;
                $("#commentSection").append(newCommentHtml);

                // Clear the textarea
                $("textarea.comment-text").val('');
            },
            error: function (xhr, status, error) {
                alert("Error posting comment. Please try again.");
            }
        });
    });

    // Event listener for the "Edit Comment" button
    $(document).on("click", ".edit-comment", function () {
        var commentId = $(this).data("comment-id");
        var commentTextElement = $(this).siblings("p.card-text");
        var commentText = commentTextElement.text();

        // Replace the comment text with an editable textarea
        commentTextElement.replaceWith(`
            <textarea class="form-control edit-comment-text">${commentText}</textarea>
            <button class="btn btn-sm btn-success save-comment" data-comment-id="${commentId}">Save</button>
            <button class="btn btn-sm btn-secondary cancel-edit">Cancel</button>
        `);
    });

    // Event listener for the "Save Comment" button
    $(document).on("click", ".save-comment", function () {
        var commentId = $(this).data("comment-id");
        var updatedText = $(this).siblings("textarea.edit-comment-text").val();

        if (updatedText.trim() === "") {
            alert("Comment cannot be empty!");
            return;
        }

        $.ajax({
            url: "/Comment/EditComment",
            type: "POST",
            data: { commentId: commentId, text: updatedText },
            success: function (response) {
                if (response.success) {
                    // Replace the textarea with the updated comment text
                    var commentHtml = `
                        <p class="card-text">${updatedText}</p>
                        <button class="btn btn-sm btn-warning edit-comment" data-comment-id="${commentId}">Edit</button>
                    `;
                    $("#comment-" + commentId + " .card-body").html(commentHtml);
                } else {
                    alert("Failed to update the comment.");
                }
            },
            error: function (xhr, status, error) {
                alert("Error updating comment. Please try again.");
            }
        });
    });

    // Event listener for the "Cancel Edit" button
    $(document).on("click", ".cancel-edit", function () {
        var commentId = $(this).siblings(".save-comment").data("comment-id");
        var originalText = $(this).siblings("textarea.edit-comment-text").val();

        // Replace the textarea with the original comment text
        $(this).closest(".card-body").html(`
            <p class="card-text">${originalText}</p>
            <button class="btn btn-sm btn-warning edit-comment" data-comment-id="${commentId}">Edit</button>
        `);
    });

    // Event listener for the "Delete Comment" button
    $(document).on("click", ".delete-comment", function () {
        var commentId = $(this).data("comment-id");

        if (confirm("Are you sure you want to delete this comment?")) {
            $.ajax({
                url: "/Comment/DeleteComment",
                type: "POST",
                data: { commentId: commentId },
                success: function (response) {
                    if (response.success) {
                        $("#comment-" + commentId).remove(); 
                    } else {
                        alert("Failed to delete the comment.");
                    }
                },
                error: function (xhr, status, error) {
                    alert("Error deleting comment. Please try again.");
                }
            });
        }
    });
});