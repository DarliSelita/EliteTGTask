$(document).ready(function () {
    let skip = 10; // Initially skipping first 10 posts

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
});

$(document).ready(function () {
    $("#submitComment").click(function () {
        let postId = @Model.Id;
        let text = $("#commentText").val();

        if (text.trim() === "") {
            alert("Comment cannot be empty!");
            return;
        }

        $.ajax({
            url: "/Comment/AddComment",
            type: "POST",
            data: { postId: postId, text: text },
            success: function (comment) {
                $("#commentSection").append(`
                    <div class="card my-2">
                        <div class="card-body">
                            <h6 class="card-subtitle text-muted">${comment.username}</h6>
                            <p class="card-text">${comment.text}</p>
                        </div>
                    </div>
                `);
                $("#commentText").val(""); // Clear input field
            },
            error: function () {
                alert("Error posting comment. Try again.");
            }
        });
    });
});
