var LunchOptionButton = React.createClass({
    render: function () {
        var clazz = this.props.userVotedForThis
            ? "btn btn-active " + this.props.activeClass
            : "btn btn-default";
        var imgurl = "/Content/" + this.props.image;
        return (
            <a href="#" onClick={this.vote} role="button" className={clazz} aria-pressed={this.props.userVotedForThis} disabled={!this.props.userInPoll}>
                <img src={imgurl} width="32" height="32"/>
            </a>
        );
    },
    vote: function() { this.props.vote(!this.props.userVotedForThis ? this.props.score : 0) }
});

var LunchOption = React.createClass({
    render: function () {
        var props = this.props;
        var upvote = function () { props.voteForThis(1); },
            downvote = function () { props.voteForThis(-1); };
        var userUpvoted = props.Upvotes.indexOf(props.username) > -1,
            userDownvoted = props.Downvotes.indexOf(props.username) > -1;

        var upvoteButton = <LunchOptionButton score={1} activeClass={"btn-success"} image={"thumbsup.png"}
                                              userVotedForThis={userUpvoted} userInPoll={props.userInPoll} vote={props.voteForThis} />,
            downvoteButton = <LunchOptionButton score={-1} activeClass={"btn-danger"} image={"thumbsdown.png"}
                                                userVotedForThis={userDownvoted} userInPoll={props.userInPoll} vote={props.voteForThis} />;

        return (
            <div className="row">
                <div className="col-xs-12">
                    <div className="panel panel-default">
                        <div className="panel-body vertical-align">
                            <div className="col-xs-2">
                                <span className="h3">{props.Score}</span><br />
                                {this.displayVotes("+", props.Upvotes)}/{this.displayVotes("-", props.Downvotes)}
                            </div>
                            <div className="col-xs-6"><span className="h3">{this.props.Name}</span></div>
                            <div className="col-xs-4">
                                <div className="btn-group btn-group-lg btn-group-justified" role="group">
                                    {downvoteButton}
                                    {upvoteButton}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        );
    },
    displayVotes: function (prefix, votes) {
        var title = votes.join("\n");
        var text = prefix + votes.length;
        return <span className="has-tooltip" data-toggle="tooltip" data-placement="bottom" data-original-title={title} title={title}>{text}</span>
    }
});

var PollInfo = React.createClass({
    render: function () {
        var goon = this.props.userIsGoon,
            inPoll = this.props.userInPoll;
        if (!inPoll)
            var notification = (goon)
                ? <strong className="text-danger">Only approved OTSS members may freely join lunch crews.</strong>
                : <strong className="text-danger">Only members are allowed to vote.</strong>
        var button = inPoll
            ? <button onClick={this.props.leave} className="btn btn-default" type="button">Leave Crew</button>
            : <button onClick={this.props.join} disabled={goon} className="btn btn-default" type="button">Join Crew</button>;
        var text = "(" + this.props.Voters.length + " member" + (this.props.Voters.length == 1 ? "" : "s") + ")";
        var voters = this.props.Voters.join("\n");
        return (
            <div id="current-poll-info" className="row vertical-align">
                <div className="col-xs-5 text-left">
                    <span className="h2">
                        {this.props.Name}&nbsp;
                        <small className="has-tooltip" data-toggle="tooltip" data-placement="bottom" title={voters} data-original-title={voters}>
                            {text}
                        </small>
                    </span>
                </div>
                <div className="col-xs-5 text-right">
                    {notification}
                </div>
                <div className="col-xs-2 text-right">
                    {button}
                </div>
            </div>
        );
    }
});

var Poll = React.createClass({
    render: function () {
        var self = this,
            api = this.props.api,
            id = this.props.Id,
            joinThis = function () { api.join(id); },
            leaveThis = function () { api.leave(id); };

        var username = this.props.username,
            userIsGoon = this.props.userIsGoon,
            userInPoll = this.props.Info.Voters.indexOf(username) > -1;

        var options = Object.keys(this.props.Options).map(function (opId) {
            var option = self.props.Options[opId];
            var voteForThis = function (score) {
                api.vote(id, option.Name, score);
            }
            return (
                <LunchOption {...option} key={opId} username={username} userInPoll={userInPoll} voteForThis={voteForThis} />
            );
        });
        return (
            <div>
                <PollInfo {...this.props.Info} userIsGoon={userIsGoon} userInPoll={userInPoll} join={joinThis} leave={leaveThis} />
                <div className="row text-center">
                    <div id="suggestions" className="col-md-12">
                        {options}
                    </div>
                </div>
            </div>
        );
    }
});

var NavbarItem = React.createClass({
    render: function () {
        var cn = this.props.isActive ? "active" : "";
        return (
            <li role="presentation" className={cn}>
                <a href="#" onClick={this.props.onClick}>{this.props.children}</a>
            </li>
        );
    }
});

var Page = React.createClass({
    getInitialState: function () {
        return {
            polls: this.props.initialPolls,
            selected: this.props.initialSelected
        };
    },
    componentDidMount: function () {
        var self = this;
        var lunchHub = $.connection.lunchHub;

        lunchHub.client.onVote = function (option) {
            var newPolls = $.extend(true, {}, self.state.polls);
            if (option.Upvotes.length !== 0 || option.Downvotes.length !== 0)
                newPolls[option.PollId].Options[option.Id] = option;
            else
                delete newPolls[option.PollId].Options[option.Id];
            self.setState({ polls: newPolls });
        };

        lunchHub.client.onPollChanged = function (poll) {
            var newPolls = $.extend(true, {}, self.state.polls);
            newPolls[poll.Id] = poll;
            self.setState({
                polls: newPolls,
                selected: poll.Info.Voters.indexOf(self.props.username) > -1
                    ? poll.Id
                    : self.state.selected
            });
        };

        lunchHub.client.onOptionDeleted = function (optionId) {
            var newPolls = $.extend(true, {}, self.state.polls);
            for (var id in newPolls) {
                delete newPolls[id].Options[optionId];
            }
            self.setState({ polls: newPolls });
        };

        lunchHub.client.onPollDeleted = function (id) {
            var newPolls = $.extend(true, {}, self.state.polls);
            delete newPolls[id];
            self.setState({
                polls: newPolls,
                selected: self.state.selected === id
                    ? null
                    : self.state.selected
            });
        };

        $.connection.hub.start();
    },
    componentDidUpdate: function () {
        $('#current-poll .has-tooltip').tooltip();
    },
    selectPoll: function (pollId) {
        this.setState({ selected: pollId });
    },
    createPoll: function (e) {
        e.preventDefault();
        this.api.create($("#new-poll").val());
        $('#new-poll').val('');
    },
    suggestOption: function (e) {
        e.preventDefault();
        var pollId = this.state.selected,
            optionName = $("#option-box").val();
        this.api.vote(pollId, optionName, 1);
        $('#option-box').typeahead('val', '');
    },
    api: {
        vote: function (pollId, name, score) {
            if (!pollId || !name) return;
            $.ajax("/api/Lunch/" + pollId + "/Vote", {
                method: "PUT",
                data: {
                    Name: name,
                    Score: score
                }
            });
        },
        create: function(name) {
            if (!name) return;
            $.ajax("/api/Lunch/" + name, {
                method: "POST"
            });
        },
        join: function (pollId) {
            $.ajax("/api/Lunch/" + pollId + "/Join", {
                method: "PUT"
            });
        },
        leave: function (pollId) {
            $.ajax("/api/Lunch/" + pollId + "/Leave", {
                method: "PUT"
            });
        }
    },
    render: function () {
        var self = this;
        var current = self.state.polls[self.state.selected];

        var poll = (current)
            ? <Poll {...current} api={this.api} username={this.props.username} userIsGoon={this.props.userIsGoon} />
            : <div className="alert alert-info">Create or join a lunch crew to start voting</div>;

        var optionFormStyle = (current && current.Info.Voters.indexOf(self.props.username) > -1)
            ? { }
            : { display: "none" };

        var navItems = Object.keys(self.state.polls).map(function (pollId) {
            var pollInfo = self.state.polls[pollId].Info,
                isActive = pollId == self.state.selected,
                chooseThis = function () { self.selectPoll(pollId); };
            return (
                <NavbarItem key={pollId} isActive={isActive} onClick={chooseThis}>
                    {pollInfo.Name}
                </NavbarItem>
            );
        });
        if (!this.props.userIsGoon) {
            var newPollFocus = function () { $("#new-poll").focus(); };
            navItems.push(
                <NavbarItem key="new" onClick={newPollFocus}>
                    <form onSubmit={self.createPoll}>
                        <input id="new-poll" type="text" placeholder="New Crew" />
                    </form>
                </NavbarItem>
            );
        };

        return (
            <div className="row">
                <nav className="col-sm-3 col-xs-12">
                    <ul className="lunch-nav nav nav-pills nav-stacked">
                        {navItems}
                    </ul>
                </nav>
                <hr className="xs" />
                <div className="vr-sm col-sm-9 col-xs-12">
                    <div id="current-poll">
                        {poll}
                    </div>
                    <div className="row">
                        <div className="col-xs-12">
                            <form id="option-form" className="form-inline" style={optionFormStyle} onSubmit={this.suggestOption}>
                                <div className="form-group">
                                    <div className="input-group">
                                        <input id="option-box" type="search" maxLength="80" placeholder="Where do we want to eat?" className="form-control lunch-option" />
                                        <span className="input-group-btn">
                                            <button type="submit" className="btn btn-default">Submit</button>
                                        </span>
                                    </div>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
});