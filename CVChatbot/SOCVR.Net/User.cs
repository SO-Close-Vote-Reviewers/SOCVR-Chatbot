using System;
using System.Net;
using System.Text;
using CsQuery;
using System.Collections.Generic;



namespace SOCVRDotNet
{
    public class User
    {
        private int currentPageNo = 1;

        /// <summary>
        /// The ID number of the user.
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// A list of ReviewItems that have been previously loaded.
        /// </summary>
        public List<ReviewItem> Reviews { get; private set; }



        public User(int id, int initialPageCount = 10)
        {
            ID = id;
            Reviews = new List<ReviewItem>();

            LoadMoreReviews(initialPageCount);
        }



        /// <summary>
        /// Scraps a user's activity tab for CV review data.
        /// </summary>
        /// <param name="pageCount">The number of pages to scrap.</param>
        public void LoadMoreReviews(int pageCount = 10)
        {
            for (var i = 0; i < pageCount; i++, currentPageNo++)
            {
                var pageHtml = new WebClient { Encoding = Encoding.UTF8 }.DownloadString("https://stackoverflow.com/ajax/users/tab/" + ID + "?tab=activity&sort=reviews&page=" + currentPageNo);
                var dom = CQ.Create(pageHtml);

                foreach (var j in dom["td"])
                {
                    if (j.FirstElementChild == null || String.IsNullOrEmpty(j.FirstElementChild.Attributes["href"]) || !j.FirstElementChild.Attributes["href"].StartsWith(@"/review/close/")) { continue; }

                    var url = j.FirstElementChild.Attributes["href"];
                    var reviewID = url.Remove(0, url.LastIndexOf('/') + 1);

                    Reviews.Add(new ReviewItem(int.Parse(reviewID)));
                }
            }
        }

        /// <summary>
        /// Clears the Reviews list and repopulates it with "fresh" review data.
        /// </summary>
        /// <param name="pageCount">The number of pages to scrap for the fresh review data.</param>
        public void RefreshReviews(int pageCount = 10)
        {
            Reviews.Clear();
            currentPageNo = 1;

            LoadMoreReviews(pageCount);
        }
    }
}
